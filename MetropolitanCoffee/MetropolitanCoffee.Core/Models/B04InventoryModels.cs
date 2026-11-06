namespace MetropolitanCoffee.Core.Models;

// Read-only models for the four sections of B04/admin/inventory.php.
public sealed record B04InventoryOverview(int AvailableProducts, long ItemsInStock, int RunningLow, int OutOfStock);
public sealed record B04InventorySnapshot(int ProductId, string Product, string Category, string? Image, long StockQuantity);
public sealed record B04InventoryReport(int ProductId, string Product, string Category, string? Image, long TotalImported, long TotalExported);
public sealed record B04InventoryAlert(int ProductId, string Product, string Category, string? Image, int CurrentStock);
