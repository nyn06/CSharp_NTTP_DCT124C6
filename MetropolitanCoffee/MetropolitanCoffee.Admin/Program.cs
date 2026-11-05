using MetropolitanCoffee.Admin.Forms;
using MetropolitanCoffee.Data.Repositories;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin;

internal static class Program
{
    internal static AdminCatalogService Catalog { get; private set; } = null!;
    internal static B04InventoryService Inventory { get; private set; } = null!;
    internal static AdminOrderService Orders { get; private set; } = null!;
    internal static string ConnectionString { get; private set; } = string.Empty;

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var connectionString = Environment.GetEnvironmentVariable("METROPOLITANCOFFEE_CONNECTION")
            ?? "Server=localhost;Database=MetropolitanCoffeeDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        ConnectionString = connectionString;
        var auth = new AdminAuthService(new SqlCustomerUserRepository(connectionString));
        var reader = new AdminReadService(connectionString);
        Catalog = new AdminCatalogService(new SqlAdminCatalogRepository(connectionString));
        Inventory = new B04InventoryService(new SqlB04InventoryRepository(connectionString));
        Orders = new AdminOrderService(new SqlAdminOrderRepository(connectionString));
        Application.Run(new LoginForm(auth, reader));
    }
}
