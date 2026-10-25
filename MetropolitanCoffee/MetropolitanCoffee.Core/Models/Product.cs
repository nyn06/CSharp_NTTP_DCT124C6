namespace MetropolitanCoffee.Core.Models;

public sealed class Product
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? Ingredients { get; init; }
    public string? Image { get; init; }
    public decimal SellPrice { get; init; }
    public int Quantity { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool HasSize { get; init; }
    public bool HasIce { get; init; }
    public bool HasSugar { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsSoldOut => Quantity <= 0 || Status == "inactive";
}
