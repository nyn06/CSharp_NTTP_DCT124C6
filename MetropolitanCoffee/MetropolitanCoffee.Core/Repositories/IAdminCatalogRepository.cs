using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface IAdminCatalogRepository
{
    Task<IReadOnlyList<AdminCategory>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminProduct>> GetProductsAsync(CancellationToken ct = default);
    Task<AdminProduct?> GetProductAsync(int id, CancellationToken ct = default);
    Task<AdminPricingSummary> GetPricingSummaryAsync(CancellationToken ct = default);
    Task<CatalogWriteResult> AddCategoryAsync(string name, CancellationToken ct = default);
    Task<CatalogWriteResult> UpdateCategoryAsync(int id, string name, CancellationToken ct = default);
    Task<CatalogWriteResult> DeleteCategoryAsync(int id, CancellationToken ct = default);
    Task<CatalogWriteResult> AddProductAsync(AdminProductInput input, CancellationToken ct = default);
    Task<CatalogWriteResult> UpdateProductAsync(int id, AdminProductInput input, CancellationToken ct = default);
    Task<CatalogWriteResult> DeleteOrHideProductAsync(int id, CancellationToken ct = default);
    Task<decimal?> UpdateProfitAsync(int id, decimal profitPercent, CancellationToken ct = default);
}
