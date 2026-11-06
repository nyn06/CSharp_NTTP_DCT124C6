using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;
using MetropolitanCoffee.Web.Models;

namespace MetropolitanCoffee.Web.Controllers;

public sealed class AccountController(CustomerAuthService auth, CustomerAccountService account,
    IWebHostEnvironment environment, ILogger<AccountController> logger) : Controller
{
    private int CustomerId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

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
        await SignInCustomerAsync(user);
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

    [Authorize(Roles = "user"), HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var profile = await account.GetProfileAsync(CustomerId, ct);
        return profile is null ? RedirectToAction(nameof(Login)) : View(profile);
    }

    [Authorize(Roles = "user"), HttpGet]
    public async Task<IActionResult> EditProfile(CancellationToken ct)
    {
        var profile = await account.GetProfileAsync(CustomerId, ct);
        return profile is null ? RedirectToAction(nameof(Login)) : View(new ProfileEditViewModel
        {
            FirstName = profile.FirstName, LastName = profile.LastName, Phone = profile.Phone,
            Ward = profile.Ward, Address = profile.Address
        });
    }

    [Authorize(Roles = "user"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(ProfileEditViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var error = await account.UpdateProfileAsync(CustomerId,
            new CustomerProfileUpdate(model.FirstName, model.LastName, model.Phone, model.Ward, model.Address), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        TempData["AccountNotice"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "user"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAvatar(IFormFile? avatar, CancellationToken ct)
    {
        if (avatar is null)
        {
            TempData["AccountError"] = "Please choose an image.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            await using var stream = avatar.OpenReadStream();
            var result = await account.UpdateAvatarAsync(CustomerId, stream, avatar.Length, environment.WebRootPath, ct);
            TempData[result.Success ? "AccountNotice" : "AccountError"] = result.Message;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Avatar upload failed for customer {CustomerId}", CustomerId);
            TempData["AccountError"] = "Unable to update your avatar. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "user"), HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize(Roles = "user"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return PasswordError("Please fill in all password fields.");
        var result = await account.ChangePasswordAsync(CustomerId, model.CurrentPassword, model.NewPassword,
            model.ConfirmNewPassword, ct);
        if (result.User is null) return PasswordError(result.Message);
        await SignInCustomerAsync(result.User);
        TempData["AccountNotice"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult PasswordError(string message)
    {
        ModelState.Clear();
        ModelState.AddModelError(string.Empty, message);
        return View("ChangePassword", new ChangePasswordViewModel());
    }

    private async Task SignInCustomerAsync(CustomerUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "user"),
            new Claim("AuthVersion", user.AuthVersion.ToString())
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }

    [Authorize(Roles = "user"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home")!;
    }
}
