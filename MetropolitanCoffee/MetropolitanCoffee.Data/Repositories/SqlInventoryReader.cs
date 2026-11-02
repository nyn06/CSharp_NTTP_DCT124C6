using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Data.Inventory;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlInventoryReader(string connectionString)
{
    public async Task<InventorySnapshot> ReadAsync()
    {
        const string sql = """
            SELECT p.Id, p.Name, c.Name, p.Quantity, p.CreatedAt, p.Status
            FROM dbo.Products p JOIN dbo.Categories c ON c.Id=p.CategoryId ORDER BY p.Name,p.Id;
            SELECT Id, ProductId, MovementType, QuantityDelta, OccurredAt, OrderItemId, PurchaseOrderItemId, Note
            FROM dbo.StockMovements ORDER BY OccurredAt DESC, Id DESC;
            SELECT i.Id,i.ProductId,po.CreatedAt,i.Quantity,po.Status,po.CreatedAt FROM dbo.PurchaseOrderItems i
            JOIN dbo.PurchaseOrders po ON po.Id=i.OrderId;
            SELECT i.Id,i.ProductId,o.CreatedAt,i.Quantity,o.Status,o.UpdatedAt FROM dbo.OrderItems i
            JOIN dbo.Orders o ON o.Id=i.OrderId;
            """;
        await using var db = new SqlConnection(connectionString);
        await db.OpenAsync();
        using var transaction = db.BeginTransaction(System.Data.IsolationLevel.Serializable);
        await using var cmd = new SqlCommand(sql, db, transaction);
        await using var rows = await cmd.ExecuteReaderAsync();
        var products = new List<InventoryProduct>();
        while (await rows.ReadAsync())
            products.Add(new(rows.GetInt32(0), rows.GetString(1), rows.GetString(2), rows.GetInt32(3), Utc(rows.GetDateTime(4)), rows.GetString(5) == "active"));
        await rows.NextResultAsync();
        var movements = new List<InventoryMovement>();
        while (await rows.ReadAsync())
            movements.Add(new(rows.GetInt64(0), rows.GetInt32(1), rows.GetString(2), rows.GetInt32(3), Utc(rows.GetDateTime(4)),
                rows.IsDBNull(5) ? null : rows.GetInt32(5), rows.IsDBNull(6) ? null : rows.GetInt32(6),
                rows.IsDBNull(7) ? "" : rows.GetString(7)));
        var documents = new List<StockDocument>();
        foreach (bool purchase in new[] { true, false })
        {
            await rows.NextResultAsync();
            while (await rows.ReadAsync())
                documents.Add(new(rows.GetInt32(0), rows.GetInt32(1), purchase, rows.GetInt32(3),
                    rows.GetString(4), rows.GetDateTime(2), rows.GetDateTime(5)));
        }
        await rows.CloseAsync();
        transaction.Commit();
        return new(products, movements, documents);
    }

    private static DateTime Utc(DateTime date) => DateTime.SpecifyKind(date, DateTimeKind.Utc);
}
