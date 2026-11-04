
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Data.Repositories;
using MetropolitanCoffee.Services;
using Xunit;

namespace MetropolitanCoffee.Tests;

public sealed class PurchaseOrderCompletionTests
{
    [Fact]
    public async Task CompleteAsync_AlreadyCompletedOrder_ReturnsInvalidStatus()
    {
        // Chỉ cho phép chạy kiểm thử trên database thử nghiệm.
        string? connectionString =
            Environment.GetEnvironmentVariable(
                "METROPOLITANCOFFEE_CONNECTION");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Chưa thiết lập chuỗi kết nối SQL Server.");

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
            connectionString);

        Assert.True(
            string.Equals(
                builder.InitialCatalog,
                "MetropolitanCoffeeDB_Test",
                StringComparison.OrdinalIgnoreCase),
            "Chỉ được kiểm thử trên MetropolitanCoffeeDB_Test.");

        var service = new PurchaseOrderService(
            new SqlPurchaseOrderRepository(connectionString!));

        // Phiếu #2 đã được hoàn tất ở bước 25.
        var result = await service.CompleteAsync(2);

        // Không được hoàn tất phiếu đã completed.
        Assert.Equal(
            PurchaseWriteStatus.InvalidStatus,
            result.Status);
    }
}
