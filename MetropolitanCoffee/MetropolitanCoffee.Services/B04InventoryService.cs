using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Data.Repositories;

namespace MetropolitanCoffee.Services;

/// <summary>Only the four read-only Inventory sections present in the original B04 website.</summary>
public sealed class B04InventoryService(SqlB04InventoryRepository repository)
{
    public Task<B04InventoryOverview> GetOverviewAsync(int threshold, CancellationToken ct = default)
    {
        ValidateThreshold(threshold);
        return repository.GetOverviewAsync(threshold, ct);
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default)
        => repository.GetCategoriesAsync(ct);

    public Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken ct = default)
        => repository.GetProductNamesAsync(ct);

    public Task<IReadOnlyList<B04InventorySnapshot>> GetSnapshotAsync(DateTime asOf, string? category, CancellationToken ct = default)
    {
        ValidateDate(asOf);
        return repository.GetSnapshotAsync(asOf, category, ct);
    }

    public Task<IReadOnlyList<B04InventoryReport>> GetReportAsync(DateTime from, DateTime to, string? productSearch, CancellationToken ct = default)
    {
        ValidateDate(from);
        ValidateDate(to);
        if (from.Date > to.Date) throw new ArgumentException("From date must be earlier than or equal to To date.");
        return repository.GetReportAsync(from, to, productSearch, ct);
    }

    public Task<IReadOnlyList<B04InventoryAlert>> GetLowStockAsync(int threshold, CancellationToken ct = default)
    {
        ValidateThreshold(threshold);
        return repository.GetLowStockAsync(threshold, ct);
    }

    private static void ValidateDate(DateTime date)
    {
        if (date.Date > DateTime.Today) throw new ArgumentException("Cannot select future dates.");
    }

    private static void ValidateThreshold(int threshold)
    {
        if (threshold is < 0 or > 999) throw new ArgumentOutOfRangeException(nameof(threshold));
    }
}
