namespace MetropolitanCoffee.Core.Models;

public sealed class CartLine
{
    public long Id { get; init; }
    public int ProductId { get; init; }
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
    public string Size { get; init; } = "none";
    public string Ice { get; init; } = "none";
    public string Sugar { get; init; } = "none";
    public int Quantity { get; init; }
    public int AvailableQuantity { get; init; }
    public string Status { get; init; } = "";
    public bool HasSize { get; init; }
    public bool HasIce { get; init; }
    public bool HasSugar { get; init; }
    public bool IsSoldOut => Status != "active" || AvailableQuantity <= 0;
    public decimal LineTotal => IsSoldOut ? 0 : Price * Quantity;
}
