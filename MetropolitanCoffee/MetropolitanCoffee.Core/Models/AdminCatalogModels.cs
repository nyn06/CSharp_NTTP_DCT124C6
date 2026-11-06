namespace MetropolitanCoffee.Core.Models;

public sealed record AdminCategory(int Id, string Name, int ProductCount);

public sealed record AdminProduct(
    int Id, int CategoryId, string Category, string Name, string? ShortDescription,
    string? Description, string? Ingredients, string? Image, decimal CostPrice,
    decimal ProfitPercent, decimal SellPrice, int Quantity, string Status,
    bool HasSize, bool HasIce, bool HasSugar);

public sealed record AdminProductInput(
    int CategoryId, string Name, string? ShortDescription, string? Description,
    string? Ingredients, string? Image, decimal ProfitPercent, string Status,
    bool HasSize, bool HasIce, bool HasSugar);

public sealed record AdminPricingSummary(decimal AverageProfit, decimal HighestSell, decimal LowestSell);

public enum CatalogWriteStatus { Saved, NotFound, Duplicate, InUse, Deleted, Hidden }

public sealed record CatalogWriteResult(CatalogWriteStatus Status, int? Id = null);
