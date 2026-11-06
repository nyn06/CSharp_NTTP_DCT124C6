
using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlAdminOrderRepository(string connectionString)
    : IAdminOrderRepository
{
    public async Task<bool> UpdateStatusAsync(
        int orderId,
        string currentStatus,
        string newStatus,
        CancellationToken ct = default)
    {
        if (orderId <= 0)
            return false;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        const string sql = """
            UPDATE dbo.Orders
            SET Status = @NewStatus,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @OrderId
              AND Status = @CurrentStatus
                AND (
                    (
                        @CurrentStatus = N'pending'
                        AND @NewStatus IN (
                            N'confirmed', N'shipping', N'delivered'
                        )
                    )
                    OR
                    (
                        @CurrentStatus = N'confirmed'
                        AND @NewStatus IN (
                            N'shipping', N'delivered'
                        )
                    )
                    OR
                    (
                        @CurrentStatus = N'shipping'
                        AND @NewStatus = N'delivered'
                    )
                )
            """;

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
        command.Parameters.Add("@CurrentStatus", SqlDbType.NVarChar, 20)
            .Value = currentStatus;
        command.Parameters.Add("@NewStatus", SqlDbType.NVarChar, 20)
            .Value = newStatus;

        return await command.ExecuteNonQueryAsync(ct) == 1;
    }


public async Task<bool> CancelOrderAsync(
    int orderId,
    CancellationToken ct = default)
{
    if (orderId <= 0)
        return false;

    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync(ct);

    await using var transaction = (SqlTransaction)
        await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, ct);

    try
    {
        // Only pending orders can be cancelled.
        await using (var command = new SqlCommand("""
            UPDATE dbo.Orders
            SET Status = N'cancelled',
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @OrderId
                AND Status IN (N'pending', N'confirmed')
            """, connection, transaction))
        {
            command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

            if (await command.ExecuteNonQueryAsync(ct) != 1)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                return false;
            }
        }

        var items = new List<(int Id, int ProductId, int Quantity)>();

        await using (var command = new SqlCommand("""
            SELECT Id, ProductId, Quantity
            FROM dbo.OrderItems
            WHERE OrderId = @OrderId
            ORDER BY Id
            """, connection, transaction))
        {
            command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

            await using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                items.Add((
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2)
                ));
            }
        }

        foreach (var item in items)
        {
            await using var movement = new SqlCommand("""
                INSERT INTO dbo.StockMovements
                    (ProductId, MovementType, QuantityDelta, OrderItemId)
                VALUES
                    (@ProductId, N'sale_cancel', @Quantity, @ItemId)
                """, connection, transaction);

            movement.Parameters.Add("@ProductId", SqlDbType.Int)
                .Value = item.ProductId;
            movement.Parameters.Add("@Quantity", SqlDbType.Int)
                .Value = item.Quantity;
            movement.Parameters.Add("@ItemId", SqlDbType.Int)
                .Value = item.Id;

            await movement.ExecuteNonQueryAsync(ct);
        }

        foreach (var group in items.GroupBy(x => x.ProductId))
        {
            await using var stock = new SqlCommand("""
                UPDATE dbo.Products
                SET Quantity = Quantity + @Quantity
                WHERE Id = @ProductId
                """, connection, transaction);

            stock.Parameters.Add("@ProductId", SqlDbType.Int)
                .Value = group.Key;
            stock.Parameters.Add("@Quantity", SqlDbType.Int)
                .Value = group.Sum(x => x.Quantity);

            if (await stock.ExecuteNonQueryAsync(ct) != 1)
                throw new InvalidOperationException(
                    "Cannot restore order inventory.");
        }

        await transaction.CommitAsync(ct);
        return true;
    }
    catch
    {
        await transaction.RollbackAsync(CancellationToken.None);
        throw;
    }
}

}
