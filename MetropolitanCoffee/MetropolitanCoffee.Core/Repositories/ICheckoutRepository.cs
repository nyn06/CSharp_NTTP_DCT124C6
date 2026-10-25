using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface ICheckoutRepository
{
    Task<CheckoutProfile> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<CheckoutPreview> PrepareAsync(int userId, BuyNowSelection? buyNow, CancellationToken ct = default);
    Task<CheckoutResult> PlaceAsync(int userId, BuyNowSelection? buyNow, Guid requestId, decimal quotedTotal, ShippingInfo shipping, string payment, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerOrder>> GetOrdersAsync(int userId, CancellationToken ct = default);
    Task<CustomerOrder?> GetOrderAsync(int userId, int orderId, CancellationToken ct = default);
}
