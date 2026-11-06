using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

/// <summary>Read-only equivalents of B04 admin/index.php and check_new_order.php.</summary>
public sealed class SqlAdminDashboardRepository(string connectionString) : IAdminDashboardRepository
{
    // Count every row, including admin users, hidden products and cancelled orders, as B04 does.
    internal const string SnapshotSql = """
        SELECT (SELECT COUNT(*) FROM dbo.Users),
               (SELECT COUNT(*) FROM dbo.Products),
               COUNT(*), COALESCE(MAX(Id), 0), MAX(UpdatedAt)
        FROM dbo.Orders
        """;
    internal const string OrdersSql = """
        SELECT COUNT(*), COALESCE(MAX(Id), 0), MAX(UpdatedAt)
        FROM dbo.Orders
        """;

    public async Task<AdminDashboardSnapshot> GetSnapshotAsync(CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(SnapshotSql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new(reader.GetInt32(0), reader.GetInt32(1), ReadOrders(reader, 2));
    }

    public async Task<AdminDashboardOrders> GetOrdersAsync(CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(OrdersSql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return ReadOrders(reader, 0);
    }

    private static AdminDashboardOrders ReadOrders(SqlDataReader reader, int offset) =>
        new(reader.GetInt32(offset), reader.GetInt32(offset + 1),
            reader.IsDBNull(offset + 2) ? null : reader.GetDateTime(offset + 2));
}
