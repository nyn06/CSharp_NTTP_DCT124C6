namespace MetropolitanCoffee.Core.Models;

public sealed record BuyNowSelection(int UserId, int ProductId, int Quantity, string Size, string Ice, string Sugar);
public sealed record CheckoutProfile(string FirstName, string LastName, string Phone, string Ward, string Address);
public sealed record ShippingInfo(string FirstName, string LastName, string Address, string Ward, string Note);

public sealed class CheckoutLine
{
    public int ProductId { get; init; }
    public string Name { get; init; } = "";
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string Size { get; init; } = "none";
    public string Ice { get; init; } = "none";
    public string Sugar { get; init; } = "none";
    public int AvailableQuantity { get; init; }
    public string Status { get; init; } = "";
    public decimal LineTotal => Price * Quantity;
}

public sealed class CheckoutPreview
{
    public IReadOnlyList<CheckoutLine> Items { get; init; } = [];
    public IReadOnlyList<string> Issues { get; init; } = [];
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public bool CanPlace => Items.Count > 0 && Issues.Count == 0;
}

public sealed record CheckoutResult(bool Success, int OrderId = 0, string? Error = null, IReadOnlyList<string>? Issues = null, bool Replayed = false);

public sealed class CustomerOrder
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = "";
    public string PaymentMethod { get; init; } = "";
    public string ReceiverName { get; init; } = "";
    public string ReceiverPhone { get; init; } = "";
    public string ShipCity { get; init; } = "";
    public string ShipWard { get; init; } = "";
    public string ShipAddress { get; init; } = "";
    public string Note { get; init; } = "";
    public IReadOnlyList<CheckoutLine> Items { get; init; } = [];
}
