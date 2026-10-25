using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default);
}
