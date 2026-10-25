using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class CartService(ICartRepository repository)
{
    public Task<CartSnapshot> GetAsync(int userId, CancellationToken ct = default) => repository.GetAsync(userId, ct);

    public Task<CartOperation> AddAsync(int userId, int productId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct = default)
    {
        if (userId <= 0 || productId <= 0) return Task.FromResult(new CartOperation(false, "Invalid product."));
        if (quantity <= 0) return Task.FromResult(new CartOperation(false, "Quantity must be at least 1."));
        return repository.AddAsync(userId, productId, quantity, size, ice, sugar, ct);
    }

    public Task<CartOperation> UpdateAsync(int userId, long lineId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct = default)
    {
        if (userId <= 0 || lineId <= 0) return Task.FromResult(new CartOperation(false, "Invalid cart item."));
        if (quantity <= 0) return Task.FromResult(new CartOperation(false, "Quantity must be at least 1."));
        return repository.UpdateAsync(userId, lineId, quantity, size, ice, sugar, ct);
    }

    public Task<CartOperation> RemoveAsync(int userId, long lineId, CancellationToken ct = default)
        => userId <= 0 || lineId <= 0
            ? Task.FromResult(new CartOperation(false, "Invalid cart item."))
            : repository.RemoveAsync(userId, lineId, ct);

    public Task<int> SyncOutOfStockAsync(int userId, CancellationToken ct = default)
        => repository.SyncOutOfStockAsync(userId, ct);
}
