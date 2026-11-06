
using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface IPurchaseOrderRepository
{
    Task<IReadOnlyList<PurchaseOrderSummary>> GetPurchaseOrdersAsync(
        CancellationToken ct = default);

    Task<IReadOnlyList<PurchaseProduct>> GetProductsAsync(
        CancellationToken ct = default);

    Task<PurchaseOrder?> GetPurchaseOrderAsync(
        int id,
        CancellationToken ct = default);

    Task<PurchaseWriteResult> CreateAsync(
        PurchaseOrderInput input,
        CancellationToken ct = default);

    Task<PurchaseWriteResult> UpdateAsync(
        int id,
        PurchaseOrderInput input,
        CancellationToken ct = default);

    Task<PurchaseWriteResult> CompleteAsync(
        int id,
        CancellationToken ct = default);
}
