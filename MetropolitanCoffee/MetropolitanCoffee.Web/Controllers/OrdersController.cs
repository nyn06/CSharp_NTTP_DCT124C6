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
}
