
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Data.Repositories;
using MetropolitanCoffee.Services;
using Microsoft.Data.SqlClient;
using Xunit;

namespace MetropolitanCoffee.Tests;

public sealed class PurchaseOrderRollbackTests
{
    [Fact]
    public async Task CompleteAsync_Overflow_RollsBackEntireTransaction()
    {
        string? connectionString =
            Environment.GetEnvironmentVariable(
                "METROPOLITANCOFFEE_CONNECTION");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Chua thiet lap chuoi ket noi SQL Server.");

        var builder = new SqlConnectionStringBuilder(connectionString);

        Assert.True(
            string.Equals(
                builder.InitialCatalog,
                "MetropolitanCoffeeDB_RollbackTest",
                StringComparison.OrdinalIgnoreCase),
            "Chi duoc kiem thu tren MetropolitanCoffeeDB_RollbackTest.");

        var service = new PurchaseOrderService(
            new SqlPurchaseOrderRepository(connectionString!));

        // Kiem tra du lieu truoc khi chay.
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        async Task<(int Quantity, decimal Cost)> ReadProduct(int id)
        {
            await using var command = new SqlCommand(
                "SELECT Quantity, CostPrice FROM dbo.Products WHERE Id = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);

            await using var reader = await command.ExecuteReaderAsync();

            Assert.True(await reader.ReadAsync());

            return (reader.GetInt32(0), reader.GetDecimal(1));
        }

        async Task<int> ReadMovementCount()
        {
            await using var command = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.StockMovements",
                connection);

            return (int)(await command.ExecuteScalarAsync())!;
        }

        var beforeCoffee = await ReadProduct(1);
        var beforeMilkTea = await ReadProduct(2);
        var beforeMovements = await ReadMovementCount();

        Assert.Equal(10, beforeCoffee.Quantity);
        Assert.Equal(2.00m, beforeCoffee.Cost);
        Assert.Equal(int.MaxValue, beforeMilkTea.Quantity);
        Assert.Equal(3.00m, beforeMilkTea.Cost);

        // San pham thu hai gay OverflowException.
        await Assert.ThrowsAsync<OverflowException>(
            () => service.CompleteAsync(1));

        // Kiem tra du lieu sau rollback.
        var afterCoffee = await ReadProduct(1);
        var afterMilkTea = await ReadProduct(2);
        var afterMovements = await ReadMovementCount();

        Assert.Equal(beforeCoffee, afterCoffee);
        Assert.Equal(beforeMilkTea, afterMilkTea);
        Assert.Equal(beforeMovements, afterMovements);

        // Phieu nhap phai giu trang thai pending.
        await using var statusCommand = new SqlCommand(
            "SELECT Status FROM dbo.PurchaseOrders WHERE Id = 1",
            connection);

        var status = (string?)await statusCommand.ExecuteScalarAsync();

        Assert.Equal("pending", status);
    }
}
