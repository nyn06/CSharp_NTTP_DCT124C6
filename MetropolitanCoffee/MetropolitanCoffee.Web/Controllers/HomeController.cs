using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Services;
using MetropolitanCoffee.Web.Models;

namespace MetropolitanCoffee.Web.Controllers;

public class HomeController : Controller
{
    private readonly CatalogService _catalog;
    private readonly ILogger<HomeController> _logger;

    public HomeController(CatalogService catalog, ILogger<HomeController> logger)
    {
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _catalog.GetSignaturePicksAsync(cancellationToken));

    public async Task<IActionResult> Menu(
        string? name, int? categoryId, decimal? minPrice, decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var categories = await _catalog.GetCategoriesAsync(cancellationToken);
        var products = await _catalog.GetProductsAsync(cancellationToken);
        var hasFilters = !string.IsNullOrWhiteSpace(name) || categoryId is not null ||
                         minPrice is not null || maxPrice is not null;
        var results = hasFilters
            ? await _catalog.SearchProductsAsync(name, categoryId, minPrice, maxPrice, cancellationToken)
            : Array.Empty<MetropolitanCoffee.Core.Models.Product>();
        return View(new CatalogViewModel
        {
            Categories = categories,
            Products = products,
            SearchResults = results,
            SearchName = name,
            SelectedCategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        });
    }

    public async Task<IActionResult> Product(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return NotFound();

        try
        {
            var product = await _catalog.GetProductByIdAsync(id, cancellationToken);
            return product is null ? NotFound() : View(product);
        }
        catch (SqlException exception)
        {
            _logger.LogError(exception, "Unable to load product {ProductId}", id);
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("ProductUnavailable");
        }
    }
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

