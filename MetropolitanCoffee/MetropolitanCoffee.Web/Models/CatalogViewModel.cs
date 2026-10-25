using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Web.Models;

public sealed class CatalogViewModel
{
    public IReadOnlyList<Category> Categories { get; init; } = Array.Empty<Category>();
    public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();
    public IReadOnlyList<Product> SearchResults { get; init; } = Array.Empty<Product>();
    public string? SearchName { get; init; }
    public int? SelectedCategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool HasFilters => !string.IsNullOrWhiteSpace(SearchName) ||
        SelectedCategoryId is not null || MinPrice is not null || MaxPrice is not null;
}
