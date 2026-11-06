using System.Data;
using Microsoft.Data.SqlClient;

namespace MetropolitanCoffee.Data.Repositories;

public sealed record InventoryHistoryRow(
    DateTime EventAt,
    int ProductId,
    string Product,
    string MovementType,
    long QuantityDelta,
    string Source,
    long ReferenceId
);

public sealed record InventoryBalanceRow(
    int ProductId,
    string Product,
    long CurrentStock,
    long LedgerBalance,
    long Difference,
    string Status
);

public sealed class SqlInventoryAuditReader
{
    private readonly string _connectionString;

    public SqlInventoryAuditReader(string connectionString)
    {
        _connectionString = connectionString;
    }

    // DES-18: Read-only movement history.
    // Legacy documents are labeled separately from ledger entries.
    // Opening balances are excluded from transaction history.
    public async Task<IReadOnlyList<InventoryHistoryRow>> GetHistoryAsync(
        DateTime from,
        DateTime to,
        string? productSearch = null,
        CancellationToken ct = default)
    {
        if (from.Date > to.Date || to.Date > DateTime.Today)
            throw new ArgumentException("Invalid date range.");

        const string sql = """
            WITH Migration AS (
                SELECT MIN(OccurredAt) AS StartedAt
                FROM dbo.StockMovements
                WHERE MovementType = N'opening'
            ),
            History AS (
                SELECT sm.OccurredAt AS EventAt,
                       p.Id AS ProductId,
                       p.Name AS Product,
                       CONVERT(NVARCHAR(40), sm.MovementType) AS MovementType,
                       CONVERT(BIGINT, sm.QuantityDelta) AS QuantityDelta,
                       N'Ledger' AS Source,
                       sm.Id AS ReferenceId
                FROM dbo.StockMovements sm
                JOIN dbo.Products p ON p.Id = sm.ProductId
                WHERE sm.MovementType <> N'opening'

                UNION ALL

                SELECT CONVERT(DATETIME2(0), po.CreatedAt),
                       p.Id,
                       p.Name,
                       N'purchase_document',
                       CONVERT(BIGINT, poi.Quantity),
                       N'Legacy document',
                       CONVERT(BIGINT, poi.Id)
                FROM dbo.PurchaseOrderItems poi
                JOIN dbo.PurchaseOrders po ON po.Id = poi.OrderId
                JOIN dbo.Products p ON p.Id = poi.ProductId
                CROSS JOIN Migration m
                WHERE po.Status = N'completed'
                  AND (
                      m.StartedAt IS NULL
                      OR CONVERT(DATETIME2(0), po.CreatedAt) < m.StartedAt
                  )

                UNION ALL

                SELECT o.CreatedAt,
                       p.Id,
                       p.Name,
                       N'sale_document',
                       -CONVERT(BIGINT, oi.Quantity),
                       N'Legacy document',
                       CONVERT(BIGINT, oi.Id)
                FROM dbo.OrderItems oi
                JOIN dbo.Orders o ON o.Id = oi.OrderId
                JOIN dbo.Products p ON p.Id = oi.ProductId
                CROSS JOIN Migration m
                WHERE o.Status <> N'cancelled'
                  AND (
                      m.StartedAt IS NULL
                      OR o.CreatedAt < m.StartedAt
                  )
            )
            SELECT TOP (500)
                   EventAt,
                   ProductId,
                   Product,
                   MovementType,
                   QuantityDelta,
                   Source,
                   ReferenceId
            FROM History
            WHERE EventAt >= @From
              AND EventAt < DATEADD(DAY, 1, @To)
              AND (
                  @Search IS NULL
                  OR Product LIKE N'%' + @Search + N'%'
              )
            ORDER BY EventAt DESC, Source, ReferenceId DESC;
            """;

        var result = new List<InventoryHistoryRow>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@From", SqlDbType.Date).Value = from.Date;
        command.Parameters.Add("@To", SqlDbType.Date).Value = to.Date;
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value =
            string.IsNullOrWhiteSpace(productSearch)
                ? DBNull.Value
                : productSearch.Trim();

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(new InventoryHistoryRow(
                reader.GetDateTime(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt64(4),
                reader.GetString(5),
                reader.GetInt64(6)
            ));
        }

        return result;
    }

    // DES-22: Compare Products.Quantity with the movement ledger.
    // Historical B04 documents must NOT be added again.
    public async Task<IReadOnlyList<InventoryBalanceRow>> GetReconciliationAsync(
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.Id,
                   p.Name,
                   CONVERT(BIGINT, p.Quantity) AS CurrentStock,
                   COALESCE(m.LedgerBalance, CONVERT(BIGINT, 0)) AS LedgerBalance,
                   CONVERT(BIGINT, p.Quantity)
                     - COALESCE(m.LedgerBalance, CONVERT(BIGINT, 0)) AS Difference,
                   CASE
                       WHEN COALESCE(m.EntryCount, 0) = 0
                           THEN N'No Ledger'
                       WHEN CONVERT(BIGINT, p.Quantity)
                            <> COALESCE(m.LedgerBalance, CONVERT(BIGINT, 0))
                           THEN N'Mismatch'
                       ELSE N'Balance OK'
                   END AS Status
            FROM dbo.Products p
            OUTER APPLY (
                SELECT SUM(CONVERT(BIGINT, QuantityDelta)) AS LedgerBalance,
                       COUNT_BIG(*) AS EntryCount
                FROM dbo.StockMovements sm
                WHERE sm.ProductId = p.Id
            ) m
            ORDER BY p.Id;
            """;

        var result = new List<InventoryBalanceRow>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(new InventoryBalanceRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt64(2),
                reader.GetInt64(3),
                reader.GetInt64(4),
                reader.GetString(5)
            ));
        }

        return result;
    }
}