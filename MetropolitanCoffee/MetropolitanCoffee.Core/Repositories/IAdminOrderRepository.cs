
namespace MetropolitanCoffee.Core.Repositories;

public interface IAdminOrderRepository
{
    Task<bool> UpdateStatusAsync(
        int orderId,
        string currentStatus,
        string newStatus,
        CancellationToken ct = default);

    Task<bool> CancelOrderAsync(
        int orderId,
        CancellationToken ct = default);
}
