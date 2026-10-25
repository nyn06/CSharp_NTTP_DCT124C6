using System.Text.RegularExpressions;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class CheckoutService(ICheckoutRepository orders, IProductRepository products)
{
    public Task<CheckoutProfile> GetProfileAsync(int userId, CancellationToken ct = default) => orders.GetProfileAsync(userId, ct);
    public Task<CheckoutPreview> PrepareAsync(int userId, BuyNowSelection? buyNow, CancellationToken ct = default) => orders.PrepareAsync(userId, buyNow, ct);
    public Task<IReadOnlyList<CustomerOrder>> GetOrdersAsync(int userId, CancellationToken ct = default) => orders.GetOrdersAsync(userId, ct);
    public Task<CustomerOrder?> GetOrderAsync(int userId, int orderId, CancellationToken ct = default) => orders.GetOrderAsync(userId, orderId, ct);

    public async Task<(BuyNowSelection? Selection, string? Error)> PrepareBuyNowAsync(int userId, int productId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct = default)
    {
        if (productId <= 0 || quantity <= 0) return (null, "Invalid product or quantity.");
        var product = await products.GetActiveByIdAsync(productId, ct);
        if (product is null) return (null, "This product is no longer available.");
        if (product.Quantity <= 0) return (null, "This product is out of stock.");
        if (quantity > product.Quantity) return (null, $"Only {product.Quantity} item(s) are available right now.");
        var options = CartOptions.ForAdd(size, ice, sugar, product.HasSize, product.HasIce, product.HasSugar);
        return (new BuyNowSelection(userId, productId, quantity, options.Size, options.Ice, options.Sugar), null);
    }

    public Task<CheckoutResult> PlaceAsync(int userId, BuyNowSelection? buyNow, Guid requestId, decimal quotedTotal, ShippingInfo shipping, string payment, CancellationToken ct = default)
    {
        shipping = new ShippingInfo(shipping.FirstName.Trim(), shipping.LastName.Trim(), shipping.Address.Trim(), shipping.Ward.Trim(), shipping.Note.Trim());
        if (requestId == Guid.Empty) return Task.FromResult(new CheckoutResult(false, Error: "Invalid checkout request."));
        if (new[] { shipping.FirstName, shipping.LastName, shipping.Address, shipping.Ward }.Any(string.IsNullOrWhiteSpace))
            return Task.FromResult(new CheckoutResult(false, Error: "Please fill all required fields!"));
        if (shipping.FirstName.Length > 50 || shipping.LastName.Length > 50 || shipping.FirstName.Length + shipping.LastName.Length + 1 > 100 ||
            !Regex.IsMatch(shipping.FirstName, @"^[\p{L}\s]+$") || !Regex.IsMatch(shipping.LastName, @"^[\p{L}\s]+$"))
            return Task.FromResult(new CheckoutResult(false, Error: "Names can only contain letters and spaces."));
        if (shipping.Address.Length > 255 || !Regex.IsMatch(shipping.Address, @"^[\p{L}0-9\s/\-]+$"))
            return Task.FromResult(new CheckoutResult(false, Error: "Please enter a valid address."));
        if (shipping.Ward.Length > 100 || !Regex.IsMatch(shipping.Ward, @"^[A-Za-z\s]+$"))
            return Task.FromResult(new CheckoutResult(false, Error: "Please select a valid ward."));
        if (shipping.Note.Length > 255) return Task.FromResult(new CheckoutResult(false, Error: "Note is too long."));
        payment = payment.Trim().ToUpperInvariant();
        if (payment is not ("COD" or "BANK" or "ONLINE"))
            return Task.FromResult(new CheckoutResult(false, Error: "Please select a payment method!"));
        return orders.PlaceAsync(userId, buyNow, requestId, quotedTotal, shipping, payment, ct);
    }
}
