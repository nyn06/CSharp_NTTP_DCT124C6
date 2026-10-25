using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface ICartRepository
{
    Task<CartSnapshot> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<CartOperation> AddAsync(int userId, int productId, int quantity, string? size, string? ice, string? sugar, CancellationToken cancellationToken = default);
    Task<CartOperation> UpdateAsync(int userId, long lineId, int quantity, string? size, string? ice, string? sugar, CancellationToken cancellationToken = default);
    Task<CartOperation> RemoveAsync(int userId, long lineId, CancellationToken cancellationToken = default);
    Task<int> SyncOutOfStockAsync(int userId, CancellationToken cancellationToken = default);
}
