using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlAdminCatalogRepository(string connectionString) : IAdminCatalogRepository
{
    private const string ProductSelect = """
        SELECT p.Id, p.CategoryId, c.Name, p.Name, p.ShortDescription, p.Description,
               p.Ingredients, p.Image, p.CostPrice, p.ProfitPercent, p.SellPrice,
               p.Quantity, p.Status, p.HasSize, p.HasIce, p.HasSugar
        FROM dbo.Products p JOIN dbo.Categories c ON c.Id=p.CategoryId
        """;

    public async Task<IReadOnlyList<AdminCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT c.Id, c.Name, COUNT(p.Id) AS ProductCount
            FROM dbo.Categories c LEFT JOIN dbo.Products p ON p.CategoryId=c.Id
            GROUP BY c.Id, c.Name ORDER BY c.Name, c.Id
            """;
        var result = new List<AdminCategory>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add(new AdminCategory(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        return result;
    }

    public async Task<IReadOnlyList<AdminProduct>> GetProductsAsync(CancellationToken ct = default)
    {
        var result = new List<AdminProduct>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(ProductSelect + " ORDER BY p.CreatedAt DESC, p.Id DESC", connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) result.Add(ReadProduct(reader));
        return result;
    }

    public async Task<AdminProduct?> GetProductAsync(int id, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(ProductSelect + " WHERE p.Id=@Id", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadProduct(reader) : null;
    }

    public async Task<AdminPricingSummary> GetPricingSummaryAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT COALESCE(AVG(CONVERT(DECIMAL(18,4),ProfitPercent)),0),
                   COALESCE(MAX(SellPrice),0), COALESCE(MIN(SellPrice),0)
            FROM dbo.Products WHERE Status=N'active'
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new AdminPricingSummary(reader.GetDecimal(0), reader.GetDecimal(1), reader.GetDecimal(2));
    }

    public async Task<CatalogWriteResult> AddCategoryAsync(string name, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("INSERT INTO dbo.Categories (Name) OUTPUT INSERTED.Id VALUES (@Name)", connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
        try { return new(CatalogWriteStatus.Saved, (int)(await command.ExecuteScalarAsync(ct))!); }
        catch (SqlException ex) when (IsDuplicate(ex)) { return new(CatalogWriteStatus.Duplicate); }
    }

    public async Task<CatalogWriteResult> UpdateCategoryAsync(int id, string name, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("UPDATE dbo.Categories SET Name=@Name WHERE Id=@Id", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
        try { return new(await command.ExecuteNonQueryAsync(ct) == 1 ? CatalogWriteStatus.Saved : CatalogWriteStatus.NotFound, id); }
        catch (SqlException ex) when (IsDuplicate(ex)) { return new(CatalogWriteStatus.Duplicate); }
    }

    public async Task<CatalogWriteResult> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            await using var check = NewCommand(connection, transaction, """
                SELECT c.Id, (SELECT COUNT(*) FROM dbo.Products p WHERE p.CategoryId=c.Id)
                FROM dbo.Categories c WITH (UPDLOCK,HOLDLOCK) WHERE c.Id=@Id
                """);
            check.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            await using var reader = await check.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return new(CatalogWriteStatus.NotFound);
            var productCount = reader.GetInt32(1);
            await reader.DisposeAsync();
            if (productCount > 0) return new(CatalogWriteStatus.InUse);
            await using var delete = NewCommand(connection, transaction, "DELETE FROM dbo.Categories WHERE Id=@Id");
            delete.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            await delete.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);
            return new(CatalogWriteStatus.Deleted, id);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            await transaction.RollbackAsync(ct);
            return new(CatalogWriteStatus.InUse);
        }
    }

    public async Task<CatalogWriteResult> AddProductAsync(AdminProductInput input, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.Products
              (CategoryId,Name,ShortDescription,Description,Ingredients,Image,
               CostPrice,ProfitPercent,Quantity,Status,HasSize,HasIce,HasSugar)
            OUTPUT INSERTED.Id
            SELECT @CategoryId,@Name,@ShortDescription,@Description,@Ingredients,@Image,
                   0,@ProfitPercent,0,@Status,@HasSize,@HasIce,@HasSugar
            WHERE EXISTS (SELECT 1 FROM dbo.Categories WHERE Id=@CategoryId)
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        AddProductParameters(command, input);
        try
        {
            var id = await command.ExecuteScalarAsync(ct);
            return id is int productId ? new(CatalogWriteStatus.Saved, productId) : new(CatalogWriteStatus.NotFound);
        }
        catch (SqlException ex) when (ex.Number == 547) { return new(CatalogWriteStatus.NotFound); }
    }

    public async Task<CatalogWriteResult> UpdateProductAsync(int id, AdminProductInput input, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.Products SET CategoryId=@CategoryId, Name=@Name,
                ShortDescription=@ShortDescription, Description=@Description,
                Ingredients=@Ingredients, Image=@Image, ProfitPercent=@ProfitPercent,
                Status=@Status, HasSize=@HasSize, HasIce=@HasIce, HasSugar=@HasSugar
            WHERE Id=@Id AND EXISTS (SELECT 1 FROM dbo.Categories WHERE Id=@CategoryId)
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        AddProductParameters(command, input);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        try { return new(await command.ExecuteNonQueryAsync(ct) == 1 ? CatalogWriteStatus.Saved : CatalogWriteStatus.NotFound, id); }
        catch (SqlException ex) when (ex.Number == 547) { return new(CatalogWriteStatus.NotFound); }
    }

    public async Task<CatalogWriteResult> DeleteOrHideProductAsync(int id, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            await using var check = NewCommand(connection, transaction, """
                SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.PurchaseOrderItems WHERE ProductId=@Id)
                              OR EXISTS(SELECT 1 FROM dbo.OrderItems WHERE ProductId=@Id)
                              OR EXISTS(SELECT 1 FROM dbo.CartItems WHERE ProductId=@Id)
                              OR EXISTS(SELECT 1 FROM dbo.StockMovements WHERE ProductId=@Id)
                            THEN 1 ELSE 0 END
                FROM dbo.Products WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Id
                """);
            check.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            var used = await check.ExecuteScalarAsync(ct);
            if (used is null) return new(CatalogWriteStatus.NotFound);
            var inUse = (int)used == 1;
            await using var change = NewCommand(connection, transaction,
                inUse ? "UPDATE dbo.Products SET Status=N'inactive' WHERE Id=@Id" : "DELETE FROM dbo.Products WHERE Id=@Id");
            change.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            await change.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);
            return new(inUse ? CatalogWriteStatus.Hidden : CatalogWriteStatus.Deleted, id);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            await transaction.RollbackAsync(ct);
            await using var hide = new SqlCommand("UPDATE dbo.Products SET Status=N'inactive' WHERE Id=@Id", connection);
            hide.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            return new(await hide.ExecuteNonQueryAsync(ct) == 1 ? CatalogWriteStatus.Hidden : CatalogWriteStatus.NotFound, id);
        }
    }

    public async Task<decimal?> UpdateProfitAsync(int id, decimal profitPercent, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await using var command = NewCommand(connection, transaction, """
            UPDATE dbo.Products SET ProfitPercent=@Profit WHERE Id=@Id;
            SELECT SellPrice FROM dbo.Products WHERE Id=@Id;
            """);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        AddDecimal(command, "@Profit", profitPercent, 5, 2);
        var value = await command.ExecuteScalarAsync(ct);
        await transaction.CommitAsync(ct);
        return value is decimal price ? price : null;
    }

    private static void AddProductParameters(SqlCommand command, AdminProductInput input)
    {
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = input.CategoryId;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = input.Name;
        command.Parameters.Add("@ShortDescription", SqlDbType.NVarChar, 255).Value = DbText(input.ShortDescription);
        command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = DbText(input.Description);
        command.Parameters.Add("@Ingredients", SqlDbType.NVarChar, -1).Value = DbText(input.Ingredients);
        command.Parameters.Add("@Image", SqlDbType.NVarChar, 255).Value = DbText(input.Image);
        AddDecimal(command, "@ProfitPercent", input.ProfitPercent, 5, 2);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = input.Status;
        command.Parameters.Add("@HasSize", SqlDbType.Bit).Value = input.HasSize;
        command.Parameters.Add("@HasIce", SqlDbType.Bit).Value = input.HasIce;
        command.Parameters.Add("@HasSugar", SqlDbType.Bit).Value = input.HasSugar;
    }

    private static object DbText(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
    private static void AddDecimal(SqlCommand command, string name, decimal value, byte precision, byte scale)
    {
        var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
        parameter.Precision = precision;
        parameter.Scale = scale;
        parameter.Value = value;
    }
    private static bool IsDuplicate(SqlException ex) => ex.Number is 2601 or 2627;
    private static SqlCommand NewCommand(SqlConnection connection, SqlTransaction transaction, string sql) => new(sql, connection, transaction);
    private static AdminProduct ReadProduct(SqlDataReader reader) => new(
        reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4),
        reader.IsDBNull(5) ? null : reader.GetString(5),
        reader.IsDBNull(6) ? null : reader.GetString(6),
        reader.IsDBNull(7) ? null : reader.GetString(7),
        reader.GetDecimal(8), reader.GetDecimal(9), reader.GetDecimal(10), reader.GetInt32(11),
        reader.GetString(12), reader.GetBoolean(13), reader.GetBoolean(14), reader.GetBoolean(15));
}
