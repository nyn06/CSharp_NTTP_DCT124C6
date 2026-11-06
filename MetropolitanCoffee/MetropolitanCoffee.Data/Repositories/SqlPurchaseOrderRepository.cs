
using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlPurchaseOrderRepository(
    string connectionString) : IPurchaseOrderRepository
{

    // Lấy danh sách phiếu nhập hàng.
    public async Task<IReadOnlyList<PurchaseOrderSummary>>
        GetPurchaseOrdersAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                po.Id,
                po.CreatedAt,
                COALESCE(SUM(i.ImportPrice * i.Quantity), 0),
                po.Status
            FROM dbo.PurchaseOrders po
            LEFT JOIN dbo.PurchaseOrderItems i
                ON i.OrderId = po.Id
            GROUP BY po.Id, po.CreatedAt, po.Status
            ORDER BY po.Id DESC
            """;

        var orders = new List<PurchaseOrderSummary>();

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(ct);

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            orders.Add(new PurchaseOrderSummary(
                reader.GetInt32(0),
                reader.GetDateTime(1),
                reader.GetDecimal(2),
                reader.GetString(3)
            ));
        }

        return orders;
    }

    // Lấy danh sách sản phẩm để Admin lựa chọn.
    public async Task<IReadOnlyList<PurchaseProduct>>
        GetProductsAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, CostPrice, Quantity, Status
            FROM dbo.Products
            ORDER BY Name, Id
            """;

        var products = new List<PurchaseProduct>();

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(ct);

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            products.Add(new PurchaseProduct(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetInt32(3),
                reader.GetString(4)
            ));
        }

        return products;
    }

    // Lấy thông tin phiếu nhập và các sản phẩm.
    public async Task<PurchaseOrder?>
        GetPurchaseOrderAsync(
            int id,
            CancellationToken ct = default)
    {
        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(ct);

        const string orderSql = """
            SELECT Id, CreatedAt, Status
            FROM dbo.PurchaseOrders
            WHERE Id = @Id
            """;

        int orderId;
        DateTime createdAt;
        string status;

        await using (var command =
            new SqlCommand(orderSql, connection))
        {
            command.Parameters.Add(
                "@Id", SqlDbType.Int).Value = id;

            await using var reader =
                await command.ExecuteReaderAsync(ct);

            if (!await reader.ReadAsync(ct))
                return null;

            orderId = reader.GetInt32(0);
            createdAt = reader.GetDateTime(1);
            status = reader.GetString(2);
        }

        const string itemsSql = """
            SELECT i.Id, i.ProductId, p.Name,
                   i.Quantity, i.ImportPrice
            FROM dbo.PurchaseOrderItems i
            JOIN dbo.Products p
                ON p.Id = i.ProductId
            WHERE i.OrderId = @Id
            ORDER BY i.Id
            """;

        var items = new List<PurchaseOrderLine>();

        await using (var command =
            new SqlCommand(itemsSql, connection))
        {
            command.Parameters.Add(
                "@Id", SqlDbType.Int).Value = id;

            await using var reader =
                await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                items.Add(new PurchaseOrderLine(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetInt32(3),
                    reader.GetDecimal(4)
                ));
            }
        }

        return new PurchaseOrder(
            orderId,
            createdAt,
            status,
            items
        );
    }

    // DES-11: Tạo phiếu nhập hàng mới.
    public async Task<PurchaseWriteResult> CreateAsync(
        PurchaseOrderInput input,
        CancellationToken ct = default)
    {

        if (input.Items is null || input.Items.Count == 0)
        {
            return new PurchaseWriteResult(
                PurchaseWriteStatus.EmptyItems);
        }

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(ct);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);

        try
        {
            const string orderSql = """
                INSERT INTO dbo.PurchaseOrders (CreatedAt, Status)
                OUTPUT INSERTED.Id
                VALUES (@OrderDate, N'pending')
                """;

            int orderId;

            await using (var command =
                new SqlCommand(orderSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@OrderDate", SqlDbType.Date).Value =
                    input.OrderDate.Date;

                orderId = (int)(await command.ExecuteScalarAsync(ct))!;
            }

            const string itemSql = """
                INSERT INTO dbo.PurchaseOrderItems
                    (OrderId, ProductId, ImportPrice, Quantity)
                SELECT @OrderId, Id, @ImportPrice, @Quantity
                FROM dbo.Products
                WHERE Id = @ProductId
                """;

            foreach (var item in input.Items)
            {
                await using var command =
                    new SqlCommand(itemSql, connection, transaction);

                command.Parameters.Add(
                    "@OrderId", SqlDbType.Int).Value = orderId;

                command.Parameters.Add(
                    "@ProductId", SqlDbType.Int).Value = item.ProductId;

                command.Parameters.Add(
                    "@Quantity", SqlDbType.Int).Value = item.Quantity;

                var price = command.Parameters.Add(
                    "@ImportPrice", SqlDbType.Decimal);
                price.Precision = 10;
                price.Scale = 2;
                price.Value = item.ImportPrice;

                if (await command.ExecuteNonQueryAsync(ct) != 1)
                {
                    await transaction.RollbackAsync(ct);
                    return new(PurchaseWriteStatus.InvalidProduct);
                }
            }

            await transaction.CommitAsync(ct);

            return new(PurchaseWriteStatus.Saved, orderId);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    // DES-12, DES-13: Chỉnh sửa phiếu nhập đang chờ xử lý.
    public async Task<PurchaseWriteResult> UpdateAsync(
        int id,
        PurchaseOrderInput input,
        CancellationToken ct = default)
    {
        if (input.Items is null || input.Items.Count == 0)
            return new(PurchaseWriteStatus.EmptyItems);

        if (input.Items.GroupBy(x => x.ProductId)
            .Any(group => group.Count() > 1))
            return new(PurchaseWriteStatus.DuplicateProduct);

        if (input.Items.Any(x =>
            x.ProductId <= 0 ||
            x.Quantity <= 0 ||
            x.ImportPrice < 0 ||
            x.ImportPrice > 99999999.99m ||
            decimal.Round(x.ImportPrice, 2) != x.ImportPrice))
            return new(PurchaseWriteStatus.InvalidProduct);

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(ct);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);

        try
        {
            const string checkSql = """
                SELECT Status
                FROM dbo.PurchaseOrders WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = @Id
                """;

            string? status;

            await using (var command =
                new SqlCommand(checkSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@Id", SqlDbType.Int).Value = id;

                status = await command.ExecuteScalarAsync(ct) as string;
            }

            if (status is null)
            {
                await transaction.RollbackAsync(ct);
                return new(PurchaseWriteStatus.NotFound);
            }

            if (status != "pending")
            {
                await transaction.RollbackAsync(ct);
                return new(PurchaseWriteStatus.InvalidStatus);
            }

            // Kiểm tra toàn bộ sản phẩm trước khi thay đổi phiếu.
            const string productSql = """
                SELECT COUNT(*)
                FROM dbo.Products WITH (HOLDLOCK)
                WHERE Id = @ProductId
                """;

            foreach (var item in input.Items)
            {
                await using var command =
                    new SqlCommand(productSql, connection, transaction);

                command.Parameters.Add(
                    "@ProductId", SqlDbType.Int).Value = item.ProductId;

                int count = (int)(await command.ExecuteScalarAsync(ct))!;

                if (count != 1)
                {
                    await transaction.RollbackAsync(ct);
                    return new(PurchaseWriteStatus.InvalidProduct);
                }
            }

            // Cập nhật ngày nhập.
            const string updateOrderSql = """
                UPDATE dbo.PurchaseOrders
                SET CreatedAt = @OrderDate
                WHERE Id = @Id AND Status = N'pending'
                """;

            await using (var command =
                new SqlCommand(updateOrderSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@Id", SqlDbType.Int).Value = id;

                command.Parameters.Add(
                    "@OrderDate", SqlDbType.Date).Value =
                    input.OrderDate.Date;

                await command.ExecuteNonQueryAsync(ct);
            }

            // Thay thế các dòng chi tiết của phiếu pending.
            const string deleteSql = """
                DELETE FROM dbo.PurchaseOrderItems
                WHERE OrderId = @OrderId
                """;

            await using (var command =
                new SqlCommand(deleteSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@OrderId", SqlDbType.Int).Value = id;

                await command.ExecuteNonQueryAsync(ct);
            }

            const string insertSql = """
                INSERT INTO dbo.PurchaseOrderItems
                    (OrderId, ProductId, ImportPrice, Quantity)
                VALUES
                    (@OrderId, @ProductId, @ImportPrice, @Quantity)
                """;

            foreach (var item in input.Items)
            {
                await using var command =
                    new SqlCommand(insertSql, connection, transaction);

                command.Parameters.Add(
                    "@OrderId", SqlDbType.Int).Value = id;

                command.Parameters.Add(
                    "@ProductId", SqlDbType.Int).Value = item.ProductId;

                command.Parameters.Add(
                    "@Quantity", SqlDbType.Int).Value = item.Quantity;

                var price = command.Parameters.Add(
                    "@ImportPrice", SqlDbType.Decimal);

                price.Precision = 10;
                price.Scale = 2;
                price.Value = item.ImportPrice;

                await command.ExecuteNonQueryAsync(ct);
            }

            await transaction.CommitAsync(ct);

            return new(PurchaseWriteStatus.Saved, id);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // Transaction có thể đã kết thúc.
            }

            throw;
        }
    }

    // DES-14, DES-15, DES-16:
    // Hoàn tất phiếu, cập nhật tồn kho và giá vốn bình quân.
    public async Task<PurchaseWriteResult> CompleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(ct);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);

        try
        {
            // Khóa phiếu để tránh hoàn tất đồng thời.
            const string checkSql = """
                SELECT Status
                FROM dbo.PurchaseOrders WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = @Id
                """;

            string? status;

            await using (var command =
                new SqlCommand(checkSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@Id", SqlDbType.Int).Value = id;

                status = await command.ExecuteScalarAsync(ct) as string;
            }

            if (status is null)
            {
                await transaction.RollbackAsync(ct);
                return new(PurchaseWriteStatus.NotFound);
            }

            if (status != "pending")
            {
                await transaction.RollbackAsync(ct);
                return new(PurchaseWriteStatus.InvalidStatus);
            }

            // Đọc chi tiết phiếu nhập.
            const string itemsSql = """
                SELECT Id, ProductId, Quantity, ImportPrice
                FROM dbo.PurchaseOrderItems
                WHERE OrderId = @Id
                ORDER BY ProductId, Id
                """;

            var items = new List<(
                int ItemId,
                int ProductId,
                int Quantity,
                decimal ImportPrice)>();

            await using (var command =
                new SqlCommand(itemsSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@Id", SqlDbType.Int).Value = id;

                await using var reader =
                    await command.ExecuteReaderAsync(ct);

                while (await reader.ReadAsync(ct))
                {
                    items.Add((
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        reader.GetDecimal(3)
                    ));
                }
            }

            if (items.Count == 0)
            {
                await transaction.RollbackAsync(ct);
                return new(PurchaseWriteStatus.EmptyItems);
            }

            const string productSql = """
                SELECT Quantity, CostPrice
                FROM dbo.Products WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = @ProductId
                """;

            const string updateProductSql = """
                UPDATE dbo.Products
                SET Quantity = @Quantity,
                    CostPrice = @CostPrice
                WHERE Id = @ProductId
                """;

            const string movementSql = """
                INSERT INTO dbo.StockMovements
                    (ProductId, MovementType, QuantityDelta,
                     PurchaseOrderItemId, Note)
                VALUES
                    (@ProductId, N'purchase', @Quantity,
                     @ItemId, N'Hoàn tất phiếu nhập hàng')
                """;

            foreach (var item in items)
            {
                int oldQuantity;
                decimal oldCost;

                await using (var command =
                    new SqlCommand(productSql, connection, transaction))
                {
                    command.Parameters.Add(
                        "@ProductId", SqlDbType.Int).Value =
                        item.ProductId;

                    await using var reader =
                        await command.ExecuteReaderAsync(ct);

                    if (!await reader.ReadAsync(ct))
                    {
                        await reader.DisposeAsync();
                        await transaction.RollbackAsync(ct);
                        return new(PurchaseWriteStatus.InvalidProduct);
                    }

                    oldQuantity = reader.GetInt32(0);
                    oldCost = reader.GetDecimal(1);
                }

                // Checked ngăn tràn số lượng INT.
                int newQuantity = checked(
                    oldQuantity + item.Quantity);

                if (newQuantity <= 0)
                    throw new InvalidOperationException(
                        "Số lượng tồn kho sau nhập không hợp lệ.");

                decimal newCost =
                    oldQuantity == 0
                    ? item.ImportPrice
                    : (
                        oldQuantity * oldCost +
                        item.Quantity * item.ImportPrice
                      ) / newQuantity;

                newCost = Math.Round(
                    newCost, 2,
                    MidpointRounding.AwayFromZero);

                if (newCost < 0 || newCost > 99999999.99m)
                    throw new OverflowException(
                        "Giá vốn vượt giới hạn của database.");

                // Cập nhật số lượng và giá vốn.
                await using (var command =
                    new SqlCommand(
                        updateProductSql, connection, transaction))
                {
                    command.Parameters.Add(
                        "@ProductId", SqlDbType.Int).Value =
                        item.ProductId;

                    command.Parameters.Add(
                        "@Quantity", SqlDbType.Int).Value =
                        newQuantity;

                    var cost = command.Parameters.Add(
                        "@CostPrice", SqlDbType.Decimal);
                    cost.Precision = 10;
                    cost.Scale = 2;
                    cost.Value = newCost;

                    if (await command.ExecuteNonQueryAsync(ct) != 1)
                        throw new InvalidOperationException(
                            "Không thể cập nhật tồn kho.");
                }

                // Ghi lịch sử nhập kho cho từng dòng phiếu.
                await using (var command =
                    new SqlCommand(
                        movementSql, connection, transaction))
                {
                    command.Parameters.Add(
                        "@ProductId", SqlDbType.Int).Value =
                        item.ProductId;

                    command.Parameters.Add(
                        "@Quantity", SqlDbType.Int).Value =
                        item.Quantity;

                    command.Parameters.Add(
                        "@ItemId", SqlDbType.Int).Value =
                        item.ItemId;

                    await command.ExecuteNonQueryAsync(ct);
                }
            }

            // Chỉ đánh dấu completed sau khi xử lý toàn bộ dòng.
            const string completeSql = """
                UPDATE dbo.PurchaseOrders
                SET Status = N'completed'
                WHERE Id = @Id AND Status = N'pending'
                """;

            await using (var command =
                new SqlCommand(completeSql, connection, transaction))
            {
                command.Parameters.Add(
                    "@Id", SqlDbType.Int).Value = id;

                if (await command.ExecuteNonQueryAsync(ct) != 1)
                    throw new InvalidOperationException(
                        "Không thể hoàn tất phiếu nhập.");
            }

            await transaction.CommitAsync(ct);

            return new(PurchaseWriteStatus.Completed, id);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // Transaction có thể đã kết thúc.
            }

            throw;
        }
    }

}
