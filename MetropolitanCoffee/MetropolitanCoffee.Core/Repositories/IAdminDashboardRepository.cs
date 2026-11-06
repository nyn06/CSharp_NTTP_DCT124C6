using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardSnapshot> GetSnapshotAsync(CancellationToken ct = default);
    Task<AdminDashboardOrders> GetOrdersAsync(CancellationToken ct = default);
}
