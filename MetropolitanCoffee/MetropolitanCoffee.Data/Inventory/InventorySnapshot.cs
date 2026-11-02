namespace MetropolitanCoffee.Data.Inventory;

public sealed record InventoryProduct(int Id, string Name, string Category, int Quantity, DateTime CreatedUtc, bool Active);
public sealed record InventoryMovement(long Id, int ProductId, string Type, int Delta, DateTime AtUtc,
    int? OrderItemId, int? PurchaseItemId, string Note);
public sealed record StockDocument(int ItemId, int ProductId, bool Purchase, int Quantity, string Status, DateTime Day, DateTime Updated);
public sealed record InventorySnapshot(List<InventoryProduct> Products, List<InventoryMovement> Movements, List<StockDocument> Documents);
