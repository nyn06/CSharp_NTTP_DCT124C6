using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed record AdminCatalogResult(bool Success, string Message, int? Id = null, decimal? SellPrice = null);

public sealed class AdminCatalogService(IAdminCatalogRepository repository)
{
    public Task<IReadOnlyList<AdminCategory>> GetCategoriesAsync(CancellationToken ct = default)
        => repository.GetCategoriesAsync(ct);
    public Task<IReadOnlyList<AdminProduct>> GetProductsAsync(CancellationToken ct = default)
        => repository.GetProductsAsync(ct);
    public Task<AdminProduct?> GetProductAsync(int id, CancellationToken ct = default)
        => repository.GetProductAsync(id, ct);
    public Task<AdminPricingSummary> GetPricingSummaryAsync(CancellationToken ct = default)
        => repository.GetPricingSummaryAsync(ct);

    public async Task<AdminCatalogResult> AddCategoryAsync(string name, CancellationToken ct = default)
    {
        var error = ValidateCategoryName(name);
        if (error is not null) return new(false, error);
        var result = await repository.AddCategoryAsync(name.Trim(), ct);
        return result.Status == CatalogWriteStatus.Duplicate
            ? new(false, "A category with this name already exists.")
            : new(true, "Category added successfully.", result.Id);
    }

    public async Task<AdminCatalogResult> UpdateCategoryAsync(int id, string name, CancellationToken ct = default)
    {
        if (id <= 0) return new(false, "Select a category.");
        var error = ValidateCategoryName(name);
        if (error is not null) return new(false, error);
        var result = await repository.UpdateCategoryAsync(id, name.Trim(), ct);
        return result.Status switch
        {
            CatalogWriteStatus.Saved => new(true, "Category updated successfully.", id),
            CatalogWriteStatus.Duplicate => new(false, "A category with this name already exists."),
            _ => new(false, "Category not found.")
        };
    }

    public async Task<AdminCatalogResult> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return new(false, "Select a category.");
        var result = await repository.DeleteCategoryAsync(id, ct);
        return result.Status switch
        {
            CatalogWriteStatus.Deleted => new(true, "Category deleted successfully.", id),
            CatalogWriteStatus.InUse => new(false, "Cannot delete a category that still contains products."),
            _ => new(false, "Category not found.")
        };
    }

    public async Task<AdminCatalogResult> SaveProductAsync(int? id, AdminProductInput input, CancellationToken ct = default)
    {
        var error = ValidateProduct(input);
        if (error is not null) return new(false, error);
        var clean = input with
        {
            Name = input.Name.Trim(), ShortDescription = input.ShortDescription?.Trim(),
            Description = input.Description?.Trim(), Ingredients = input.Ingredients?.Trim()
        };
        var result = id is null
            ? await repository.AddProductAsync(clean, ct)
            : await repository.UpdateProductAsync(id.Value, clean, ct);
        return result.Status == CatalogWriteStatus.Saved
            ? new(true, id is null ? "Product added successfully." : "Product updated successfully.", result.Id)
            : new(false, id is null ? "The selected category no longer exists." : "Product or category not found.");
    }

    public async Task<AdminCatalogResult> DeleteOrHideProductAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return new(false, "Select a product.");
        var result = await repository.DeleteOrHideProductAsync(id, ct);
        return result.Status switch
        {
            CatalogWriteStatus.Deleted => new(true, "Product deleted permanently.", id),
            CatalogWriteStatus.Hidden => new(true, "Product hidden because it has related transactions or cart history.", id),
            _ => new(false, "Product not found.")
        };
    }

    public async Task<AdminCatalogResult> UpdateProfitAsync(int id, decimal profitPercent, CancellationToken ct = default)
    {
        if (id <= 0) return new(false, "Select a product.");
        var error = ValidateProfit(profitPercent);
        if (error is not null) return new(false, error);
        decimal? price;
        try { price = await repository.UpdateProfitAsync(id, profitPercent, ct); }
        catch (OverflowException)
        {
            return new(false, "Sell price exceeds the database limit (99,999,999.99). Reduce the profit percentage.");
        }
        return price is null
            ? new(false, "Product not found.")
            : new(true, "Pricing updated successfully.", id, price);
    }

    public static decimal CalculateSellPrice(decimal costPrice, decimal profitPercent)
        => Math.Round(costPrice * (1 + profitPercent / 100m), 2, MidpointRounding.AwayFromZero);

    private static string? ValidateCategoryName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Category name is required.";
        if (name.Trim().Length > 100) return "Category name must be 100 characters or fewer.";
        return null;
    }

    private static string? ValidateProduct(AdminProductInput input)
    {
        if (input.CategoryId <= 0) return "Select a category.";
        if (string.IsNullOrWhiteSpace(input.Name)) return "Product name is required.";
        if (input.Name.Trim().Length > 200) return "Product name must be 200 characters or fewer.";
        if (input.ShortDescription?.Length > 255) return "Short description must be 255 characters or fewer.";
        if (input.Image?.Length > 255 || input.Image is not null && Path.GetFileName(input.Image) != input.Image)
            return "Invalid product image name.";
        if (input.Status is not ("active" or "inactive")) return "Select a valid product status.";
        return ValidateProfit(input.ProfitPercent);
    }

    private static string? ValidateProfit(decimal profitPercent)
    {
        if (profitPercent < 0 || profitPercent > 999.99m) return "Profit percentage must be between 0 and 999.99.";
        if (decimal.Round(profitPercent, 2) != profitPercent) return "Profit percentage supports up to two decimal places.";
        return null;
    }
}
