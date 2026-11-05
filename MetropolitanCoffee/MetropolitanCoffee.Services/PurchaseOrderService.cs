
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class PurchaseOrderService(
    IPurchaseOrderRepository repository)
{
    
    // Lấy danh sách phiếu nhập hàng.
    public Task<IReadOnlyList<PurchaseOrderSummary>>
        GetPurchaseOrdersAsync(CancellationToken ct = default)
        => repository.GetPurchaseOrdersAsync(ct);


    public Task<IReadOnlyList<PurchaseProduct>> GetProductsAsync(
        CancellationToken ct = default)
        => repository.GetProductsAsync(ct);

    public Task<PurchaseOrder?> GetPurchaseOrderAsync(
        int id,
        CancellationToken ct = default)
        => repository.GetPurchaseOrderAsync(id, ct);

    public Task<PurchaseWriteResult> CreateAsync(
        PurchaseOrderInput input,
        CancellationToken ct = default)
    {
        var validation = Validate(input);

        if (validation is not null)
            return Task.FromResult(validation);

        return repository.CreateAsync(input, ct);
    }

    public Task<PurchaseWriteResult> UpdateAsync(
        int id,
        PurchaseOrderInput input,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Task.FromResult(
                new PurchaseWriteResult(PurchaseWriteStatus.NotFound));

        var validation = Validate(input);

        if (validation is not null)
            return Task.FromResult(validation);

        return repository.UpdateAsync(id, input, ct);
    }

    public Task<PurchaseWriteResult> CompleteAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Task.FromResult(
                new PurchaseWriteResult(PurchaseWriteStatus.NotFound));

        return repository.CompleteAsync(id, ct);
    }

    private static PurchaseWriteResult? Validate(
        PurchaseOrderInput input)
    {
        if (input is null ||
            input.Items is null ||
            input.Items.Count == 0)
        {
            return new(PurchaseWriteStatus.EmptyItems);
        }

        if (input.OrderDate.Date > DateTime.Today ||
            input.OrderDate.Date < new DateTime(1753, 1, 1))
        {
            return new(PurchaseWriteStatus.InvalidStatus);
        }

        if (input.Items.Any(item =>
            item.ProductId <= 0 ||
            item.Quantity <= 0 ||
            item.ImportPrice < 0 ||
            item.ImportPrice > 99999999.99m ||
            decimal.Round(item.ImportPrice, 2) != item.ImportPrice))
        {
            return new(PurchaseWriteStatus.InvalidProduct);
        }

        if (input.Items
            .GroupBy(item => item.ProductId)
            .Any(group => group.Count() > 1))
        {
            return new(PurchaseWriteStatus.DuplicateProduct);
        }

        return null;
    }
}
