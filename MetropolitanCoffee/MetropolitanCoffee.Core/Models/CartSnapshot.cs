namespace MetropolitanCoffee.Core.Models;

public sealed class CartSnapshot
{
    public IReadOnlyList<CartLine> Items { get; init; } = [];
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public int TotalQuantity => Items.Where(item => !item.IsSoldOut).Sum(item => item.Quantity);
    public bool HasSoldOut => Items.Any(item => item.IsSoldOut);
}
