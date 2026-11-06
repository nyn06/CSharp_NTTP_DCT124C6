using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;
using MetropolitanCoffee.Data.Repositories;

namespace MetropolitanCoffee.Services;

/// <summary>Read-only queries for the first phase of the desktop administrator.</summary>
public sealed class AdminReadService(string connectionString, IAdminDashboardRepository? dashboardRepository = null)
{
    private readonly IAdminDashboardRepository _dashboard = dashboardRepository ?? new SqlAdminDashboardRepository(connectionString);

    public Task<AdminDashboardSnapshot> GetDashboardAsync(CancellationToken ct = default)
        => _dashboard.GetSnapshotAsync(ct);

    public Task<AdminDashboardOrders> GetDashboardOrdersAsync(CancellationToken ct = default)
        => _dashboard.GetOrdersAsync(ct);

    public async Task<(int Users, int Products, int Orders)> GetDashboardCountsAsync(CancellationToken ct = default)
    {
        var snapshot = await GetDashboardAsync(ct);
        return (snapshot.Users, snapshot.Products, snapshot.Orders.Count);
    }

    public async Task<DataTable> GetTableAsync(string section, CancellationToken ct = default)
    {
        var sql = section switch
        {
            "products" => """
                SELECT p.Id AS [Product ID], p.Name AS [Product], c.Name AS [Category],
                       p.SellPrice AS [Sell Price], p.Quantity AS [Quantity],
                       CASE p.Status WHEN 'active' THEN N'Active' ELSE N'Hidden' END AS [Status],
                       p.Image AS [Image]
                FROM dbo.Products p JOIN dbo.Categories c ON c.Id=p.CategoryId ORDER BY p.Id DESC
                """,
            "categories" => "SELECT Id AS [Category ID], Name AS [Category], CreatedAt AS [Created At] FROM dbo.Categories ORDER BY Id DESC",
            "orders" => """
                SELECT o.Id AS [Order ID], u.Username AS [Customer], o.CreatedAt AS [Order Date],
                       o.Total AS [Total], o.PaymentMethod AS [Payment], o.Status AS [Status]
                FROM dbo.Orders o JOIN dbo.Users u ON u.Id=o.UserId ORDER BY o.CreatedAt DESC, o.Id DESC
                """,
            "users" or "customers" => """
                SELECT Id AS [User ID], Username, Email,
                       CASE Status WHEN 'active' THEN N'Active' ELSE N'Locked' END AS [Status],
                       CASE Role WHEN 'admin' THEN N'Admin' ELSE N'User' END AS [Role]
                FROM dbo.Users ORDER BY Id DESC
                """,
            "purchases" => """
                SELECT po.Id AS [Purchase Order ID], po.CreatedAt AS [Order Date],
                       COALESCE(SUM(i.ImportPrice*i.Quantity),0) AS [Total Value],
                       po.Status AS [Status]
                FROM dbo.PurchaseOrders po LEFT JOIN dbo.PurchaseOrderItems i ON i.OrderId=po.Id
                GROUP BY po.Id, po.CreatedAt, po.Status ORDER BY po.Id DESC
                """,
            "stock" => """
                SELECT p.Id AS [Product ID], p.Name AS [Product], c.Name AS [Category],
                       p.Quantity AS [Quantity],
                       CASE WHEN p.Quantity<=10 THEN N'Low Stock' ELSE N'In Stock' END AS [Alert]
                FROM dbo.Products p JOIN dbo.Categories c ON c.Id=p.CategoryId ORDER BY p.Quantity, p.Id
                """,
            "movements" => """
                SELECT m.Id AS [Movement ID], m.OccurredAt AS [Date], p.Name AS [Product],
                       m.MovementType AS [Type], m.QuantityDelta AS [Change], m.Note AS [Note]
                FROM dbo.StockMovements m JOIN dbo.Products p ON p.Id=m.ProductId
                ORDER BY m.OccurredAt DESC, m.Id DESC
                """,
            "recent" => """
                SELECT TOP (8) o.Id AS [Order ID], u.Username AS [Customer], o.CreatedAt AS [Order Date],
                       o.Total AS [Total], o.Status AS [Status]
                FROM dbo.Orders o JOIN dbo.Users u ON u.Id=o.UserId ORDER BY o.CreatedAt DESC, o.Id DESC
                """,
            "pricing" => """
                SELECT p.Id AS [Product ID], p.Name AS [Product], c.Name AS [Category],
                       p.CostPrice AS [Cost Price], p.ProfitPercent AS [Profit %], p.SellPrice AS [Sell Price]
                FROM dbo.Products p JOIN dbo.Categories c ON c.Id=p.CategoryId
                ORDER BY c.Name, p.Name
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public async Task<(int Products, int Orders, decimal Revenue, int LowStock)> GetOverviewAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT (SELECT COUNT(*) FROM dbo.Products),
                   (SELECT COUNT(*) FROM dbo.Orders),
                   (SELECT COALESCE(SUM(Total),0) FROM dbo.Orders WHERE Status <> 'cancelled'),
                   (SELECT COUNT(*) FROM dbo.Products WHERE Quantity <= 10 AND Status='active')
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return (reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2), reader.GetInt32(3));
    }

    public async Task<DataTable> GetDetailItemsAsync(string section, int id, CancellationToken ct = default)
    {
        var sql = section switch
        {
            "orders" => """
                SELECT p.Name AS [Product], i.Quantity AS [Quantity], i.Price AS [Unit Price],
                       i.Quantity*i.Price AS [Subtotal], i.Size AS [Size], i.Ice AS [Ice], i.Sugar AS [Sugar]
                FROM dbo.OrderItems i JOIN dbo.Products p ON p.Id=i.ProductId
                WHERE i.OrderId=@Id ORDER BY i.Id
                """,
            "purchases" => """
                SELECT p.Name AS [Product], i.Quantity AS [Quantity], i.ImportPrice AS [Unit Cost],
                       i.Quantity*i.ImportPrice AS [Subtotal]
                FROM dbo.PurchaseOrderItems i JOIN dbo.Products p ON p.Id=i.ProductId
                WHERE i.OrderId=@Id ORDER BY i.Id
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(ct);
        var table = new DataTable();
        table.Load(reader);
        return table;
    }
}
