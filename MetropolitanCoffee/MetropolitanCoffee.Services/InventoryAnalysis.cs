using MetropolitanCoffee.Data.Inventory;

namespace MetropolitanCoffee.Services;

public sealed record InventoryReportRow(int ProductId, string Product, string Category,
    long Opening, long Imported, long Exported, long Closing, bool Estimated);
public sealed record StockCheck(int ProductId, string Product, string Check, long Expected, long Actual, string Detail);

public static class InventoryAnalysis
{
    public static bool LowStock(InventoryProduct product, int threshold) => product.Active && product.Quantity <= threshold;

    // B04 reports use receipt and order dates and their current status.
    private static int DocumentDelta(StockDocument d) => d.Purchase
        ? (d.Status == "completed" ? d.Quantity : 0)
        : (d.Status == "cancelled" ? 0 : -d.Quantity);

    private static DateTime BusinessDay(StockDocument d, DateTime? baseline)
    {
        // Legacy timestamps were copied verbatim. New checkout timestamps are UTC.
        if (d.Purchase || baseline is null || d.Day < baseline.Value) return d.Day.Date;
        return DateTime.SpecifyKind(d.Day, DateTimeKind.Utc).ToLocalTime().Date;
    }

    public static IEnumerable<InventoryReportRow> Report(InventorySnapshot data, DateTime from, DateTime through)
    {
        if (from.Date > through.Date || through.Date > DateTime.Today)
            throw new ArgumentException("Khoảng ngày không hợp lệ.");
        var baseline = data.Movements.Where(m => m.Type == "opening" &&
            m.Note.StartsWith("B04 migration current balance", StringComparison.Ordinal))
            .Select(m => (DateTime?)m.AtUtc).Min();
        var ledger = data.Movements.ToLookup(m => m.ProductId);
        var documents = data.Documents.ToLookup(d => d.ProductId);
        foreach (var p in data.Products)
        {
            if (p.CreatedUtc.Date > through.Date && !ledger[p.Id].Any(m => m.AtUtc.ToLocalTime().Date <= through.Date)) continue;
            var docs = documents[p.Id].ToList();
            var movements = ledger[p.Id].ToList();
            long BalanceBefore(DateTime boundary)
            {
                if (baseline.HasValue && boundary.ToUniversalTime() <= baseline.Value)
                {
                    // Reconstruct from the imported opening balance, never add old sales twice.
                    var anchor = movements.Where(m => m.Type == "opening" && m.Note.StartsWith("B04 migration current balance", StringComparison.Ordinal)).Sum(m => (long)m.Delta);
                    var legacy = docs.Where(d => BusinessDay(d, baseline) >= boundary.Date && d.Day < baseline.Value);
                    return anchor - legacy.Sum(d => !d.Purchase && d.Status == "cancelled" && d.Updated > baseline.Value
                        ? -(long)d.Quantity : DocumentDelta(d));
                }
                return movements.Where(m => m.AtUtc < boundary.ToUniversalTime()).Sum(m => (long)m.Delta);
            }
            var period = docs.Where(d => BusinessDay(d, baseline) >= from.Date && BusinessDay(d, baseline) <= through.Date).ToList();
            var imported = period.Where(d => d.Purchase).Sum(d => (long)DocumentDelta(d));
            var exported = period.Where(d => !d.Purchase).Sum(d => -(long)DocumentDelta(d));
            yield return new(p.Id, p.Name, p.Category, BalanceBefore(from.Date), imported, exported,
                BalanceBefore(through.Date.AddDays(1)), baseline.HasValue && from.Date.ToUniversalTime() <= baseline.Value);
        }
    }

    public static IEnumerable<StockCheck> Reconcile(InventorySnapshot data)
    {
        var ledger = data.Movements.ToLookup(m => m.ProductId);
        var baseline = data.Movements.Where(m => m.Type == "opening" && m.Note.StartsWith("B04 migration current balance", StringComparison.Ordinal))
            .Select(m => (DateTime?)m.AtUtc).Min();
        var documents = data.Documents.ToLookup(d => d.ProductId);
        foreach (var p in data.Products)
        {
            var entries = ledger[p.Id].ToList();
            yield return new(p.Id, p.Name, "Số dư", entries.Sum(m => (long)m.Delta), p.Quantity, "");
            foreach (var d in documents[p.Id])
            {
                var linked = entries.Where(m => d.Purchase ? m.PurchaseItemId == d.ItemId : m.OrderItemId == d.ItemId).ToList();
                // Imported documents are already covered by the opening balance.
                // Receipts store a date only: on the import day, missing entries cannot be attributed reliably.
                bool legacy = baseline.HasValue && (d.Purchase ? d.Day.Date <= baseline.Value.Date : d.Day <= baseline.Value);
                if (!legacy || linked.Any(m => m.Type == (d.Purchase ? "purchase" : "sale")))
                {
                    long expected = d.Purchase ? (d.Status == "completed" ? d.Quantity : 0) : -d.Quantity;
                    yield return new(p.Id, p.Name, d.Purchase ? "Nhập" : "Bán", expected,
                        linked.Where(m => m.Type == (d.Purchase ? "purchase" : "sale")).Sum(m => (long)m.Delta), $"Dòng {d.ItemId}");
                }
                if (!d.Purchase && (!legacy || d.Updated > baseline || linked.Any(m => m.Type == "sale_cancel")))
                    yield return new(p.Id, p.Name, "Hoàn khi hủy", d.Status == "cancelled" ? d.Quantity : 0,
                        linked.Where(m => m.Type == "sale_cancel").Sum(m => (long)m.Delta), $"Dòng {d.ItemId}");
            }
        }
    }
}
