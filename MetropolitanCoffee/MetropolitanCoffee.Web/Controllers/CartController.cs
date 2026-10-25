using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetropolitanCoffee.Services;
using System.Security.Claims;

namespace MetropolitanCoffee.Web.Controllers;

[Authorize(Roles = "user")]
public sealed class CartController(CartService cart) : Controller
{
    private int CustomerId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await cart.GetAsync(CustomerId, ct));

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity, string? size, string? ice, string? sugar, string? returnUrl, CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole("user"))
            return RedirectToAction("Login", "Account", new { returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Cart") });
        var result = await cart.AddAsync(CustomerId, productId, quantity, size, ice, sugar, ct);
        if (result.Success)
        {
            TempData["CartNotice"] = "Product added to cart.";
            return RedirectToAction(nameof(Index));
        }
        TempData["CartError"] = result.Error;
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index))!;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long lineId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct)
    {
        var result = await cart.UpdateAsync(CustomerId, lineId, quantity, size, ice, sugar, ct);
        if (!result.Success) TempData["CartError"] = result.Error;
        return RedirectToAction(nameof(Index))!;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(long lineId, CancellationToken ct)
    {
        var result = await cart.RemoveAsync(CustomerId, lineId, ct);
        if (!result.Success) TempData["CartError"] = result.Error;
        return RedirectToAction(nameof(Index))!;
    }

    [HttpGet]
    public async Task<IActionResult> Stock(CancellationToken ct)
    {
        var snapshot = await cart.GetAsync(CustomerId, ct);
        return Json(new { success = true, items = snapshot.Items.Select(line => new { id = line.Id, isOutOfStock = line.IsSoldOut }) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncStock(CancellationToken ct)
        => Json(new { success = true, updated = await cart.SyncOutOfStockAsync(CustomerId, ct) });
}
