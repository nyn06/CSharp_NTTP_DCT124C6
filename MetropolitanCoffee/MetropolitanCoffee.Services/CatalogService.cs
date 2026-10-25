using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class CatalogService
{
    private readonly ICategoryRepository _categories;
    private readonly IProductRepository _products;

    public CatalogService(ICategoryRepository categories, IProductRepository products)
    {
        _categories = categories;
        _products = products;
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => _categories.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
        => _products.GetActiveAsync(cancellationToken);

    public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        => id > 0 ? _products.GetActiveByIdAsync(id, cancellationToken) : Task.FromResult<Product?>(null);

    // B04 filters names, category and inclusive price limits while retaining catalog order.
    public async Task<IReadOnlyList<Product>> SearchProductsAsync(
        string? name, int? categoryId, decimal? minPrice, decimal? maxPrice,
        CancellationToken cancellationToken = default)
    {
        var products = await _products.GetActiveAsync(cancellationToken);
        var keyword = name?.Trim();
        var minimum = Math.Max(0, minPrice ?? 0);
        var maximum = maxPrice is null ? decimal.MaxValue : Math.Max(0, maxPrice.Value);
        return products.Where(product =>
            (string.IsNullOrEmpty(keyword) || product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
            (categoryId is null || product.CategoryId == categoryId) &&
            product.SellPrice >= minimum && product.SellPrice <= maximum).ToArray();
    }

    // B04 app.js: prefer Coffee, fall back to all products, newest eight.
    public async Task<IReadOnlyList<Product>> GetSignaturePicksAsync(CancellationToken cancellationToken = default)
    {
        var products = await _products.GetActiveAsync(cancellationToken);
        var coffee = products.Where(p => p.CategoryName.Equals("coffee", StringComparison.OrdinalIgnoreCase)).ToArray();
        return (coffee.Length > 0 ? coffee : products)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8).ToArray();
    }
}

