using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetropolitanCoffee.Services;
using MetropolitanCoffee.Web.Models;

namespace MetropolitanCoffee.Web.Controllers;

public sealed class AccountController(CustomerAuthService auth) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (user, error) = await auth.LoginAsync(model.Username, model.Password, ct);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "user"),
            new Claim("AuthVersion", user.AuthVersion.ToString())
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : RedirectToAction("Index", "Home")!;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var error = await auth.RegisterAsync(model.Username, model.Email, model.FirstName, model.LastName,
            model.Phone, model.Ward, model.Address, model.Password, model.ConfirmPassword, ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        TempData["AccountNotice"] = "Account created successfully! Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = "user"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home")!;
    }
}
