using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Data.Repositories;

/// <summary>
/// Read-only SQL Server implementation of B04/admin/inventory.php.
/// Snapshot deliberately reproduces B04's historical ESTIMATE from present stock and documents,
/// not a reconstructed transaction-time balance.
/// </summary>
public sealed class SqlB04InventoryRepository(string connectionString)
{
    public async Task<B04InventoryOverview> GetOverviewAsync(int threshold, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*) AS AvailableProducts,
                   COALESCE(SUM(CONVERT(BIGINT, Quantity)), 0) AS ItemsInStock,
                   COUNT(CASE WHEN Quantity > 0 AND Quantity <= @Threshold THEN 1 END) AS RunningLow,
                   COUNT(CASE WHEN Quantity = 0 THEN 1 END) AS OutOfStock
            FROM dbo.Products WHERE Status = N'active';
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Threshold", SqlDbType.Int).Value = threshold;
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new B04InventoryOverview(reader.GetInt32(0), reader.GetInt64(1), reader.GetInt32(2), reader.GetInt32(3));
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT Name FROM dbo.Categories ORDER BY Name";
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var categories = new List<string>();
        while (await reader.ReadAsync(ct)) categories.Add(reader.GetString(0));
        return categories;
    }

    public async Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT DISTINCT Name FROM dbo.Products ORDER BY Name";
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var products = new List<string>();
        while (await reader.ReadAsync(ct)) products.Add(reader.GetString(0));
        return products;
    }

    public async Task<IReadOnlyList<B04InventorySnapshot>> GetSnapshotAsync(DateTime asOf, string? category, CancellationToken ct = default)
    {
        // B04: opening = current - completed purchases (all time) + non-cancelled sales (all time).
        // estimated stock = opening + completed purchases through selected day - sales through selected day.
        // PurchaseOrders.CreatedAt is DATE in SQL Server; Orders.CreatedAt is DATETIME2.
        const string sql = """
            SELECT p.Id, p.Name, COALESCE(c.Name, N'Uncategorized'), p.Image,
                   CONVERT(BIGINT, p.Quantity) - COALESCE(imports.AllQuantity, 0)
                     + COALESCE(sales.AllQuantity, 0)
                     + COALESCE(imports.ByDateQuantity, 0) - COALESCE(sales.ByDateQuantity, 0) AS EstimatedStock
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON c.Id = p.CategoryId
            OUTER APPLY (
                SELECT SUM(CONVERT(BIGINT, poi.Quantity)) AS AllQuantity,
                       SUM(CASE WHEN po.CreatedAt <= @AsOf THEN CONVERT(BIGINT, poi.Quantity) ELSE CONVERT(BIGINT, 0) END) AS ByDateQuantity
                FROM dbo.PurchaseOrderItems poi
                JOIN dbo.PurchaseOrders po ON po.Id = poi.OrderId
                WHERE poi.ProductId = p.Id AND po.Status = N'completed'
            ) imports
            OUTER APPLY (
                SELECT SUM(CONVERT(BIGINT, oi.Quantity)) AS AllQuantity,
                       SUM(CASE WHEN o.CreatedAt < DATEADD(day, 1, @AsOf) THEN CONVERT(BIGINT, oi.Quantity) ELSE CONVERT(BIGINT, 0) END) AS ByDateQuantity
                FROM dbo.OrderItems oi
                JOIN dbo.Orders o ON o.Id = oi.OrderId
                WHERE oi.ProductId = p.Id AND o.Status <> N'cancelled'
            ) sales
            WHERE p.CreatedAt < DATEADD(day, 1, @AsOf)
              AND (@Category IS NULL OR c.Name = @Category)
            ORDER BY c.Name, EstimatedStock DESC, p.Id;
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@AsOf", SqlDbType.Date).Value = asOf.Date;
        command.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(category) ? DBNull.Value : category;
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<B04InventorySnapshot>();
        while (await reader.ReadAsync(ct)) rows.Add(new B04InventorySnapshot(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetInt64(4)));
        return rows;
    }

    public async Task<IReadOnlyList<B04InventoryReport>> GetReportAsync(DateTime from, DateTime to, string? productSearch, CancellationToken ct = default)
    {
        // B04 includes products that have at least one completed purchase or non-cancelled sale
        // in the selected date range. SQL's exclusive next-day bound includes the full To day.
        const string sql = """
            SELECT p.Id, p.Name, COALESCE(c.Name, N'Uncategorized'), p.Image,
                   COALESCE(imports.Quantity, CONVERT(BIGINT,0)),
                   COALESCE(sales.Quantity, CONVERT(BIGINT,0))
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON c.Id = p.CategoryId
            OUTER APPLY (
                SELECT SUM(CONVERT(BIGINT,poi.Quantity)) AS Quantity
                FROM dbo.PurchaseOrderItems poi JOIN dbo.PurchaseOrders po ON po.Id=poi.OrderId
                WHERE poi.ProductId=p.Id AND po.Status=N'completed'
                  AND po.CreatedAt>=@From AND po.CreatedAt<DATEADD(day,1,@To)
            ) imports
            OUTER APPLY (
                SELECT SUM(CONVERT(BIGINT,oi.Quantity)) AS Quantity
                FROM dbo.OrderItems oi JOIN dbo.Orders o ON o.Id=oi.OrderId
                WHERE oi.ProductId=p.Id AND o.Status<>N'cancelled'
                  AND o.CreatedAt>=@From AND o.CreatedAt<DATEADD(day,1,@To)
            ) sales
            WHERE (COALESCE(imports.Quantity,0)>0 OR COALESCE(sales.Quantity,0)>0)
              AND (@Search IS NULL OR p.Name LIKE N'%' + @Search + N'%')
            ORDER BY p.Name;
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@From", SqlDbType.Date).Value = from.Date;
        command.Parameters.Add("@To", SqlDbType.Date).Value = to.Date;
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(productSearch) ? DBNull.Value : productSearch.Trim();
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<B04InventoryReport>();
        while (await reader.ReadAsync(ct)) rows.Add(new B04InventoryReport(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetInt64(4), reader.GetInt64(5)));
        return rows;
    }

    public async Task<IReadOnlyList<B04InventoryAlert>> GetLowStockAsync(int threshold, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.Id, p.Name, COALESCE(c.Name,N'Uncategorized'), p.Image, p.Quantity
            FROM dbo.Products p LEFT JOIN dbo.Categories c ON c.Id=p.CategoryId
            WHERE p.Quantity<=@Threshold AND p.Status=N'active'
            ORDER BY p.Quantity ASC, p.Id ASC;
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Threshold", SqlDbType.Int).Value = threshold;
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<B04InventoryAlert>();
        while (await reader.ReadAsync(ct)) rows.Add(new B04InventoryAlert(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetInt32(4)));
        return rows;
    }
}
