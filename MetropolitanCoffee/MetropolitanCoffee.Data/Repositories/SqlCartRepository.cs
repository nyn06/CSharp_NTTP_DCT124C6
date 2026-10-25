using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlCartRepository(string connectionString) : ICartRepository
{
    private const string LineSelect = """
        SELECT c.Id, c.ProductId, p.Name, c.UnitPrice, c.Size, c.Ice, c.Sugar,
               c.Quantity, p.Quantity, p.Status, p.HasSize, p.HasIce, p.HasSugar
        FROM dbo.CartItems AS c
        INNER JOIN dbo.Products AS p ON p.Id = c.ProductId
        """;

    public async Task<CartSnapshot> GetAsync(int userId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(LineSelect + " WHERE c.UserId = @UserId ORDER BY c.AddedAt, c.Id", connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        var lines = new List<CartLine>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) lines.Add(ReadLine(reader));
        return new CartSnapshot { Items = lines };
    }

    public async Task<CartOperation> AddAsync(int userId, int productId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockUserAsync(connection, transaction, userId, ct)) return new(false, "Please login again.");
        var product = await GetProductAsync(connection, transaction, productId, ct);
        if (product is null || product.Status != "active") return new(false, "This product is no longer available.");
        if (product.Quantity <= 0) return new(false, "This product is out of stock.");

        await using (var sumCommand = NewCommand(connection, transaction,
            "SELECT COALESCE(SUM(CONVERT(bigint, Quantity)), 0) FROM dbo.CartItems WHERE UserId=@UserId AND ProductId=@ProductId"))
        {
            sumCommand.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            sumCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
            var current = (long)(await sumCommand.ExecuteScalarAsync(ct) ?? 0L);
            if (current + quantity > product.Quantity) return new(false, $"Only {product.Quantity} item(s) are available right now.");
        }

        var options = CartOptions.ForAdd(size, ice, sugar, product.HasSize, product.HasIce, product.HasSugar);
        await using (var command = NewCommand(connection, transaction, """
            UPDATE dbo.CartItems SET Quantity = Quantity + @Quantity
            WHERE UserId=@UserId AND ProductId=@ProductId AND Size=@Size AND Ice=@Ice AND Sugar=@Sugar;
            IF @@ROWCOUNT = 0
                INSERT INTO dbo.CartItems (UserId, ProductId, Size, Ice, Sugar, UnitPrice, Quantity)
                VALUES (@UserId, @ProductId, @Size, @Ice, @Sugar, @UnitPrice, @Quantity);
            """))
        {
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
            command.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = product.SellPrice;
            AddOptions(command, options);
            await command.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
        return new(true);
    }

    public async Task<CartOperation> UpdateAsync(int userId, long lineId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockUserAsync(connection, transaction, userId, ct)) return new(false, "Please login again.");
        CartLine? line;
        await using (var command = NewCommand(connection, transaction, LineSelect + " WHERE c.Id=@LineId AND c.UserId=@UserId"))
        {
            command.Parameters.Add("@LineId", SqlDbType.BigInt).Value = lineId;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            await using var reader = await command.ExecuteReaderAsync(ct);
            line = await reader.ReadAsync(ct) ? ReadLine(reader) : null;
        }
        if (line is null) return new(false, "This cart item no longer exists.");
        if (line.Status != "active") return new(false, "This product is no longer available.");
        if (line.AvailableQuantity <= 0) return new(false, "This product is out of stock.");

        long otherQuantity;
        await using (var command = NewCommand(connection, transaction,
            "SELECT COALESCE(SUM(CONVERT(bigint, Quantity)), 0) FROM dbo.CartItems WHERE UserId=@UserId AND ProductId=@ProductId AND Id<>@LineId"))
        {
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
            command.Parameters.Add("@LineId", SqlDbType.BigInt).Value = lineId;
            otherQuantity = (long)(await command.ExecuteScalarAsync(ct) ?? 0L);
        }
        var maxForLine = Math.Max(0, (long)line.AvailableQuantity - otherQuantity);
        if (quantity > maxForLine)
            return new(false, maxForLine > 0 ? $"Only {maxForLine} item(s) are available right now." : "This product is out of stock.");

        var options = CartOptions.ForUpdate(size, ice, sugar, line);
        await using (var command = NewCommand(connection, transaction, """
            DECLARE @TargetId bigint = (
                SELECT Id FROM dbo.CartItems
                WHERE UserId=@UserId AND ProductId=@ProductId AND Size=@Size AND Ice=@Ice AND Sugar=@Sugar
            );
            IF @TargetId IS NOT NULL AND @TargetId <> @LineId
            BEGIN
                UPDATE dbo.CartItems SET Quantity=Quantity+@Quantity WHERE Id=@TargetId AND UserId=@UserId;
                DELETE FROM dbo.CartItems WHERE Id=@LineId AND UserId=@UserId;
            END
            ELSE
                UPDATE dbo.CartItems SET Quantity=@Quantity, Size=@Size, Ice=@Ice, Sugar=@Sugar
                WHERE Id=@LineId AND UserId=@UserId;
            """))
        {
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
            command.Parameters.Add("@LineId", SqlDbType.BigInt).Value = lineId;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
            AddOptions(command, options);
            await command.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
        return new(true);
    }

    public async Task<CartOperation> RemoveAsync(int userId, long lineId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockUserAsync(connection, transaction, userId, ct)) return new(false, "Please login again.");
        await using var command = NewCommand(connection, transaction, "DELETE FROM dbo.CartItems WHERE Id=@LineId AND UserId=@UserId");
        command.Parameters.Add("@LineId", SqlDbType.BigInt).Value = lineId;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        var changed = await command.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return changed > 0 ? new(true) : new(false, "This cart item no longer exists.");
    }

    public async Task<int> SyncOutOfStockAsync(int userId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockUserAsync(connection, transaction, userId, ct)) return 0;
        await using var command = NewCommand(connection, transaction, """
            UPDATE c SET Quantity=0
            FROM dbo.CartItems AS c
            INNER JOIN dbo.Products AS p ON p.Id=c.ProductId
            WHERE c.UserId=@UserId AND c.Quantity<>0 AND (p.Status<>N'active' OR p.Quantity<=0)
            """);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        var updated = await command.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return updated;
    }

    private static async Task<bool> LockUserAsync(SqlConnection connection, SqlTransaction transaction, int userId, CancellationToken ct)
    {
        await using var command = NewCommand(connection, transaction,
            "SELECT Id FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE Id=@UserId AND Role=N'user' AND Status=N'active'");
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        return await command.ExecuteScalarAsync(ct) is not null;
    }

    private static async Task<CartProduct?> GetProductAsync(SqlConnection connection, SqlTransaction transaction, int productId, CancellationToken ct)
    {
        await using var command = NewCommand(connection, transaction,
            "SELECT Quantity, Status, HasSize, HasIce, HasSugar, SellPrice FROM dbo.Products WITH (UPDLOCK, HOLDLOCK) WHERE Id=@ProductId");
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? new CartProduct(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2), reader.GetBoolean(3), reader.GetBoolean(4), reader.GetDecimal(5))
            : null;
    }

    private static SqlCommand NewCommand(SqlConnection connection, SqlTransaction transaction, string sql)
        => new(sql, connection, transaction);

    private static void AddOptions(SqlCommand command, CartOptions options)
    {
        command.Parameters.Add("@Size", SqlDbType.NVarChar, 20).Value = options.Size;
        command.Parameters.Add("@Ice", SqlDbType.NVarChar, 20).Value = options.Ice;
        command.Parameters.Add("@Sugar", SqlDbType.NVarChar, 20).Value = options.Sugar;
    }

    private static CartLine ReadLine(SqlDataReader reader) => new()
    {
        Id = reader.GetInt64(0), ProductId = reader.GetInt32(1), Name = reader.GetString(2), Price = reader.GetDecimal(3),
        Size = reader.GetString(4), Ice = reader.GetString(5), Sugar = reader.GetString(6), Quantity = reader.GetInt32(7),
        AvailableQuantity = reader.GetInt32(8), Status = reader.GetString(9),
        HasSize = reader.GetBoolean(10), HasIce = reader.GetBoolean(11), HasSugar = reader.GetBoolean(12)
    };

    private sealed record CartProduct(int Quantity, string Status, bool HasSize, bool HasIce, bool HasSugar, decimal SellPrice);
}
