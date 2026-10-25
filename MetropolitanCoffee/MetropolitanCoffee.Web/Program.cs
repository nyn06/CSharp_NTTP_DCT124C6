var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
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
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();



