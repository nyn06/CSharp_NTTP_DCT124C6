using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Web.Controllers;

[Authorize(Roles = "user")]
public sealed class OrdersController(CheckoutService checkout) : Controller
{
    private int CustomerId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await checkout.GetOrdersAsync(CustomerId, ct));

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await checkout.GetOrderAsync(CustomerId, id, ct);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var cancelled = await checkout.CancelOrderAsync(CustomerId, id, ct);
        TempData[cancelled ? "OrderNotice" : "OrderError"] = cancelled
            ? "Order cancelled successfully." : "This order can no longer be cancelled.";
        return RedirectToAction(cancelled ? nameof(Details) : nameof(Index), cancelled ? new { id } : null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(int id, string? address, string? ward, CancellationToken ct)
    {
        var result = await checkout.UpdateDeliveryAddressAsync(CustomerId, id, address, ward, ct);
        TempData[result.Success ? "OrderNotice" : "OrderError"] = result.Message;
        return RedirectToAction(result.Success ? nameof(Details) : nameof(Index), result.Success ? new { id } : null);
    }
}
