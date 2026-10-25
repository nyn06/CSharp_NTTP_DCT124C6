using System.Security.Claims;
using System.Text.Json;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;
using MetropolitanCoffee.Web.Models;

namespace MetropolitanCoffee.Web.Controllers;

[Authorize(Roles = "user")]
public sealed class CheckoutController(CheckoutService checkout, ILogger<CheckoutController> logger) : Controller
{
    private const string BuyNowKey = "Checkout.BuyNow";
    private const string TokenKey = "Checkout.RequestId";
    private const string ModeKey = "Checkout.Mode";
    private const string QuoteKey = "Checkout.QuotedTotal";
    private int CustomerId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BuyNow(int productId, int quantity, string? size, string? ice, string? sugar, CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole("user"))
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Product", "Home", new { id = productId }) });
        var (selection, error) = await checkout.PrepareBuyNowAsync(CustomerId, productId, quantity, size, ice, sugar, ct);
        if (selection is null)
        {
            TempData["CheckoutError"] = error;
            return RedirectToAction("Product", "Home", new { id = productId });
        }
        HttpContext.Session.SetString(BuyNowKey, JsonSerializer.Serialize(selection));
        return RedirectToAction(nameof(Index), new { buyNow = true });
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool buyNow = false, CancellationToken ct = default)
    {
        var selection = buyNow ? ReadBuyNow() : null;
        if (buyNow && selection is null) return RedirectToAction("Menu", "Home")!;
        if (!buyNow) HttpContext.Session.Remove(BuyNowKey);
        var preview = await checkout.PrepareAsync(CustomerId, selection, ct);
        if (preview.Items.Count == 0) return RedirectToAction(buyNow ? "Menu" : "Index", buyNow ? "Home" : "Cart")!;
        var profile = await checkout.GetProfileAsync(CustomerId, ct);
        var token = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString(TokenKey, token);
        HttpContext.Session.SetString(ModeKey, buyNow ? "buy" : "cart");
        HttpContext.Session.SetString(QuoteKey, preview.Subtotal.ToString(CultureInfo.InvariantCulture));
        return View(new CheckoutPageViewModel { BuyNow = buyNow, Preview = preview, Profile = profile, RequestId = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(CheckoutPageViewModel model, CancellationToken ct)
    {
        var sessionToken = HttpContext.Session.GetString(TokenKey);
        if (!Guid.TryParse(model.RequestId, out var requestId) || sessionToken != model.RequestId ||
            HttpContext.Session.GetString(ModeKey) != (model.BuyNow ? "buy" : "cart") ||
            !decimal.TryParse(HttpContext.Session.GetString(QuoteKey), NumberStyles.Number, CultureInfo.InvariantCulture, out var quotedTotal))
            return BadRequest("Invalid or expired checkout request.");
        var selection = model.BuyNow ? ReadBuyNow() : null;
        // A repeated BUY NOW POST can still resolve to its existing order after the temporary cart was cleared.
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please fill all required fields!");
            return await RenderFailedAsync(model, selection, ct);
        }
        try
        {
            var shipping = new ShippingInfo(model.FirstName, model.LastName, model.Address, model.Ward, model.Note ?? "");
            var result = await checkout.PlaceAsync(CustomerId, selection, requestId, quotedTotal, shipping, model.PaymentMethod, ct);
            if (result.Success)
            {
                if (model.BuyNow) HttpContext.Session.Remove(BuyNowKey);
                TempData["OrderNotice"] = "Order placed successfully!";
                return RedirectToAction("Details", "Orders", new { id = result.OrderId });
            }
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to place order.");
            foreach (var issue in result.Issues ?? []) ModelState.AddModelError(string.Empty, issue);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Checkout failed for customer {CustomerId}", CustomerId);
            ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
        }
        return await RenderFailedAsync(model, selection, ct);
    }

    private async Task<IActionResult> RenderFailedAsync(CheckoutPageViewModel model, BuyNowSelection? selection, CancellationToken ct)
    {
        model.Preview = await checkout.PrepareAsync(CustomerId, selection, ct);
        HttpContext.Session.SetString(QuoteKey, model.Preview.Subtotal.ToString(CultureInfo.InvariantCulture));
        model.Profile = await checkout.GetProfileAsync(CustomerId, ct);
        return View("Index", model);
    }

    private BuyNowSelection? ReadBuyNow()
    {
        var json = HttpContext.Session.GetString(BuyNowKey);
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var selection = JsonSerializer.Deserialize<BuyNowSelection>(json);
            return selection?.UserId == CustomerId ? selection : null;
        }
        catch (JsonException) { return null; }
    }
}
