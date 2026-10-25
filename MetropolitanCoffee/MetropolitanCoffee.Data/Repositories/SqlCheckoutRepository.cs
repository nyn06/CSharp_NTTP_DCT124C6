using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlCheckoutRepository(string connectionString) : ICheckoutRepository
{
    private const string CartLinesSql = """
        SELECT c.ProductId, p.Name, p.SellPrice, c.Quantity, c.Size, c.Ice, c.Sugar, p.Quantity, p.Status
        FROM dbo.CartItems AS c
        INNER JOIN dbo.Products AS p ON p.Id=c.ProductId
        WHERE c.UserId=@UserId AND c.Quantity>0
        ORDER BY c.ProductId, c.Id
        """;

    public async Task<CheckoutProfile> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("SELECT FirstName, LastName, Phone, Ward, Address FROM dbo.Users WHERE Id=@UserId AND Role=N'user'", connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) throw new InvalidOperationException("Customer account no longer exists.");
        return new CheckoutProfile(ReadString(reader, 0), ReadString(reader, 1), ReadString(reader, 2), ReadString(reader, 3), ReadString(reader, 4));
    }

    public async Task<CheckoutPreview> PrepareAsync(int userId, BuyNowSelection? buyNow, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        var lines = await LoadLinesAsync(connection, null, userId, buyNow, false, ct);
        return BuildPreview(lines);
    }

    public async Task<CheckoutResult> PlaceAsync(int userId, BuyNowSelection? buyNow, Guid requestId, decimal quotedTotal, ShippingInfo shipping, string payment, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            string phone;
            await using (var customer = NewCommand(connection, transaction,
                "SELECT Phone FROM dbo.Users WITH (UPDLOCK,HOLDLOCK) WHERE Id=@UserId AND Role=N'user' AND Status=N'active'"))
            {
                customer.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                var value = await customer.ExecuteScalarAsync(ct);
                if (value is null) return new CheckoutResult(false, Error: "Please login again.");
                phone = value == DBNull.Value ? "" : (string)value;
            }

            await using (var replay = NewCommand(connection, transaction, "SELECT Id FROM dbo.Orders WHERE UserId=@UserId AND CheckoutToken=@Token"))
            {
                replay.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                replay.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = requestId;
                var existing = await replay.ExecuteScalarAsync(ct);
                if (existing is int existingId) return new CheckoutResult(true, existingId, Replayed: true);
            }

            var lines = await LoadLinesAsync(connection, transaction, userId, buyNow, true, ct);
            var preview = BuildPreview(lines);
            if (preview.Items.Count == 0) return new CheckoutResult(false, Error: "Your cart is empty.");
            if (preview.Issues.Count > 0)
                return new CheckoutResult(false, Error: "Some items are no longer available in the requested quantity.", Issues: preview.Issues);
            if (preview.Subtotal != quotedTotal)
                return new CheckoutResult(false, Error: "The price has changed. Please review the updated total and confirm again.");

            var paymentCode = payment switch { "BANK" => "bank", "ONLINE" => "online", _ => "cash" };
            var status = payment == "ONLINE" ? "confirmed" : "pending";
            int orderId;
            await using (var insert = NewCommand(connection, transaction, """
                INSERT INTO dbo.Orders (UserId, ReceiverName, ReceiverPhone, ShipCity, ShipWard, ShipAddress, Note, Total, PaymentMethod, Status, CheckoutToken)
                OUTPUT INSERTED.Id
                VALUES (@UserId, @Name, @Phone, N'Ho Chi Minh', @Ward, @Address, @Note, @Total, @Payment, @Status, @Token)
                """))
            {
                insert.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                insert.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = shipping.FirstName + " " + shipping.LastName;
                insert.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = phone;
                insert.Parameters.Add("@Ward", SqlDbType.NVarChar, 100).Value = shipping.Ward;
                insert.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = shipping.Address;
                insert.Parameters.Add("@Note", SqlDbType.NVarChar, 255).Value = shipping.Note;
                insert.Parameters.Add("@Total", SqlDbType.Decimal).Value = preview.Subtotal;
                insert.Parameters.Add("@Payment", SqlDbType.NVarChar, 20).Value = paymentCode;
                insert.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                insert.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = requestId;
                orderId = (int)(await insert.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("Order insert failed."));
            }

            foreach (var line in preview.Items)
            {
                int itemId;
                await using (var insertItem = NewCommand(connection, transaction, """
                    INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, Price, Size, Ice, Sugar)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderId, @ProductId, @Quantity, @Price, @Size, @Ice, @Sugar)
                    """))
                {
                    insertItem.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                    insertItem.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
                    insertItem.Parameters.Add("@Quantity", SqlDbType.Int).Value = line.Quantity;
                    insertItem.Parameters.Add("@Price", SqlDbType.Decimal).Value = line.Price;
                    insertItem.Parameters.Add("@Size", SqlDbType.NVarChar, 20).Value = line.Size;
                    insertItem.Parameters.Add("@Ice", SqlDbType.NVarChar, 20).Value = line.Ice;
                    insertItem.Parameters.Add("@Sugar", SqlDbType.NVarChar, 20).Value = line.Sugar;
                    itemId = (int)(await insertItem.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("Order item insert failed."));
                }
                await using var movement = NewCommand(connection, transaction, """
                    INSERT INTO dbo.StockMovements (ProductId, MovementType, QuantityDelta, OrderItemId)
                    VALUES (@ProductId, N'sale', @Delta, @ItemId)
                    """);
                movement.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
                movement.Parameters.Add("@Delta", SqlDbType.Int).Value = -line.Quantity;
                movement.Parameters.Add("@ItemId", SqlDbType.Int).Value = itemId;
                await movement.ExecuteNonQueryAsync(ct);
            }

            foreach (var group in preview.Items.GroupBy(line => line.ProductId).OrderBy(group => group.Key))
            {
                await using var stock = NewCommand(connection, transaction, """
                    UPDATE dbo.Products SET Quantity=Quantity-@Quantity
                    WHERE Id=@ProductId AND Status=N'active' AND Quantity>=@Quantity
                    """);
                stock.Parameters.Add("@ProductId", SqlDbType.Int).Value = group.Key;
                stock.Parameters.Add("@Quantity", SqlDbType.Int).Value = group.Sum(line => line.Quantity);
                if (await stock.ExecuteNonQueryAsync(ct) != 1)
                    throw new InvalidOperationException("Stock changed while placing the order.");
            }

            if (buyNow is null)
            {
                await using var clearCart = NewCommand(connection, transaction, "DELETE FROM dbo.CartItems WHERE UserId=@UserId");
                clearCart.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                await clearCart.ExecuteNonQueryAsync(ct);
            }
            await transaction.CommitAsync(ct);
            return new CheckoutResult(true, orderId);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<IReadOnlyList<CustomerOrder>> GetOrdersAsync(int userId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            SELECT Id, CreatedAt, Total, Status, PaymentMethod, ReceiverName, ReceiverPhone, ShipCity, ShipWard, ShipAddress, Note
            FROM dbo.Orders WHERE UserId=@UserId ORDER BY CreatedAt DESC, Id DESC
            """, connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        var orders = new List<CustomerOrder>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) orders.Add(ReadOrder(reader));
        return orders;
    }

    public async Task<CustomerOrder?> GetOrderAsync(int userId, int orderId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        CustomerOrder? order;
        await using (var command = new SqlCommand("""
            SELECT Id, CreatedAt, Total, Status, PaymentMethod, ReceiverName, ReceiverPhone, ShipCity, ShipWard, ShipAddress, Note
            FROM dbo.Orders WHERE Id=@OrderId AND UserId=@UserId
            """, connection))
        {
            command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            await using var reader = await command.ExecuteReaderAsync(ct);
            order = await reader.ReadAsync(ct) ? ReadOrder(reader) : null;
        }
        if (order is null) return null;
        var items = new List<CheckoutLine>();
        await using (var command = new SqlCommand("""
            SELECT oi.ProductId, p.Name, oi.Price, oi.Quantity, oi.Size, oi.Ice, oi.Sugar
            FROM dbo.OrderItems AS oi JOIN dbo.Products AS p ON p.Id=oi.ProductId
            WHERE oi.OrderId=@OrderId ORDER BY oi.Id
            """, connection))
        {
            command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) items.Add(new CheckoutLine
            {
                ProductId = reader.GetInt32(0), Name = reader.GetString(1), Price = reader.GetDecimal(2), Quantity = reader.GetInt32(3),
                Size = ReadString(reader, 4), Ice = ReadString(reader, 5), Sugar = ReadString(reader, 6)
            });
        }
        return new CustomerOrder
        {
            Id = order.Id, CreatedAt = order.CreatedAt, Total = order.Total, Status = order.Status,
            PaymentMethod = order.PaymentMethod, ReceiverName = order.ReceiverName, ReceiverPhone = order.ReceiverPhone,
            ShipCity = order.ShipCity, ShipWard = order.ShipWard, ShipAddress = order.ShipAddress, Note = order.Note,
            Items = items
        };
    }

    private static async Task<List<CheckoutLine>> LoadLinesAsync(SqlConnection connection, SqlTransaction? transaction, int userId, BuyNowSelection? buyNow, bool lockProducts, CancellationToken ct)
    {
        var lines = new List<CheckoutLine>();
        if (buyNow is not null)
        {
            if (buyNow.UserId != userId || buyNow.Quantity <= 0) return lines;
            var sql = "SELECT Id, Name, SellPrice, Quantity, Status FROM dbo.Products " + (lockProducts ? "WITH (UPDLOCK,HOLDLOCK) " : "") + "WHERE Id=@ProductId";
            await using var command = transaction is null ? new SqlCommand(sql, connection) : NewCommand(connection, transaction, sql);
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = buyNow.ProductId;
            await using var reader = await command.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct)) lines.Add(new CheckoutLine
            {
                ProductId = reader.GetInt32(0), Name = reader.GetString(1), Price = reader.GetDecimal(2), Quantity = buyNow.Quantity,
                Size = buyNow.Size, Ice = buyNow.Ice, Sugar = buyNow.Sugar,
                AvailableQuantity = reader.GetInt32(3), Status = reader.GetString(4)
            });
            return lines;
        }

        var cartSql = lockProducts ? CartLinesSql.Replace("dbo.CartItems AS c", "dbo.CartItems AS c WITH (UPDLOCK,HOLDLOCK)")
            .Replace("dbo.Products AS p", "dbo.Products AS p WITH (UPDLOCK,HOLDLOCK)") : CartLinesSql;
        await using var cartCommand = transaction is null ? new SqlCommand(cartSql, connection) : NewCommand(connection, transaction, cartSql);
        cartCommand.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await using var cartReader = await cartCommand.ExecuteReaderAsync(ct);
        while (await cartReader.ReadAsync(ct)) lines.Add(new CheckoutLine
        {
            ProductId = cartReader.GetInt32(0), Name = cartReader.GetString(1), Price = cartReader.GetDecimal(2),
            Quantity = cartReader.GetInt32(3), Size = cartReader.GetString(4), Ice = cartReader.GetString(5), Sugar = cartReader.GetString(6),
            AvailableQuantity = cartReader.GetInt32(7), Status = cartReader.GetString(8)
        });
        return lines;
    }

    private static CheckoutPreview BuildPreview(List<CheckoutLine> lines)
    {
        var issues = new List<string>();
        foreach (var group in lines.GroupBy(line => line.ProductId))
        {
            var line = group.First();
            if (line.Status != "active") issues.Add(line.Name + " is no longer available.");
            else if (line.AvailableQuantity <= 0) issues.Add(line.Name + " is out of stock.");
            else if (group.Sum(item => (long)item.Quantity) > line.AvailableQuantity)
                issues.Add($"{line.Name} only has {line.AvailableQuantity} left, but your cart needs {group.Sum(item => (long)item.Quantity)}.");
        }
        return new CheckoutPreview { Items = lines, Issues = issues };
    }

    private static CustomerOrder ReadOrder(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0), CreatedAt = reader.GetDateTime(1), Total = reader.GetDecimal(2),
        Status = reader.GetString(3), PaymentMethod = reader.GetString(4), ReceiverName = ReadString(reader, 5),
        ReceiverPhone = ReadString(reader, 6), ShipCity = ReadString(reader, 7), ShipWard = ReadString(reader, 8),
        ShipAddress = ReadString(reader, 9), Note = ReadString(reader, 10)
    };

    private static string ReadString(SqlDataReader reader, int index) => reader.IsDBNull(index) ? "" : reader.GetString(index);
    private static SqlCommand NewCommand(SqlConnection connection, SqlTransaction transaction, string sql) => new(sql, connection, transaction);
}
