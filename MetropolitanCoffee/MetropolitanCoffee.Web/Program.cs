var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MetropolitanCoffee.Core.Repositories.ICustomerUserRepository>(provider => new MetropolitanCoffee.Data.Repositories.SqlCustomerUserRepository(provider.GetRequiredService<IConfiguration>().GetConnectionString("MetropolitanCoffee") ?? throw new InvalidOperationException("Missing MetropolitanCoffee connection string.")));
builder.Services.AddScoped<MetropolitanCoffee.Services.CustomerAuthService>();
builder.Services.AddScoped<MetropolitanCoffee.Core.Repositories.ICartRepository>(provider => new MetropolitanCoffee.Data.Repositories.SqlCartRepository(provider.GetRequiredService<IConfiguration>().GetConnectionString("MetropolitanCoffee") ?? throw new InvalidOperationException("Missing MetropolitanCoffee connection string.")));
builder.Services.AddScoped<MetropolitanCoffee.Services.CartService>();
builder.Services.AddScoped<MetropolitanCoffee.Core.Repositories.ICheckoutRepository>(provider => new MetropolitanCoffee.Data.Repositories.SqlCheckoutRepository(provider.GetRequiredService<IConfiguration>().GetConnectionString("MetropolitanCoffee") ?? throw new InvalidOperationException("Missing MetropolitanCoffee connection string.")));
builder.Services.AddScoped<MetropolitanCoffee.Services.CheckoutService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "MetropolitanCoffee.Checkout";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.Name = "MetropolitanCoffee.Customer";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        var returnUrl = HttpMethods.IsPost(context.Request.Method) && context.Request.Path.StartsWithSegments("/Cart")
            ? "/Cart"
            : HttpMethods.IsPost(context.Request.Method) && context.Request.Path.StartsWithSegments("/Checkout")
                ? "/Checkout"
            : context.Request.PathBase.ToString() + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl));
        return Task.CompletedTask;
    };
    options.Events.OnValidatePrincipal = async context =>
    {
        var idText = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var versionText = context.Principal?.FindFirst("AuthVersion")?.Value;
        if (!int.TryParse(idText, out var id) || !long.TryParse(versionText, out var version))
        {
            context.RejectPrincipal();
            return;
        }
        var user = await context.HttpContext.RequestServices.GetRequiredService<MetropolitanCoffee.Core.Repositories.ICustomerUserRepository>()
            .FindByIdAsync(id, context.HttpContext.RequestAborted);
        if (user is null || user.Role != "user" || user.Status != "active" || user.AuthVersion != version)
        {
            context.RejectPrincipal();
            await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context.HttpContext, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<MetropolitanCoffee.Core.Repositories.ICategoryRepository>(provider => new MetropolitanCoffee.Data.Repositories.SqlCategoryRepository(provider.GetRequiredService<IConfiguration>().GetConnectionString("MetropolitanCoffee") ?? throw new InvalidOperationException("Missing MetropolitanCoffee connection string.")));
builder.Services.AddScoped<MetropolitanCoffee.Core.Repositories.IProductRepository>(provider => new MetropolitanCoffee.Data.Repositories.SqlProductRepository(provider.GetRequiredService<IConfiguration>().GetConnectionString("MetropolitanCoffee") ?? throw new InvalidOperationException("Missing MetropolitanCoffee connection string.")));
builder.Services.AddScoped<MetropolitanCoffee.Services.CatalogService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();



