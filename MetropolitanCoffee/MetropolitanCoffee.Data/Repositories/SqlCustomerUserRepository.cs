using System.Data;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlCustomerUserRepository(string connectionString) : ICustomerUserRepository
{
    public Task<CustomerUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => FindAsync("Username = @Value", username, SqlDbType.NVarChar, 50, ct);

    public Task<CustomerUser?> FindByIdAsync(int id, CancellationToken ct = default)
        => FindAsync("Id = @Value", id, SqlDbType.Int, 0, ct);

    private async Task<CustomerUser?> FindAsync(string predicate, object value, SqlDbType type, int size, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand($"SELECT Id, Username, PasswordHash, Role, Status, AuthVersion FROM dbo.Users WHERE {predicate}", connection);
        var parameter = command.Parameters.Add("@Value", type);
        if (size > 0) parameter.Size = size;
        parameter.Value = value;
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? new CustomerUser
        {
            Id = reader.GetInt32(0), Username = reader.GetString(1), PasswordHash = reader.GetString(2),
            Role = reader.GetString(3), Status = reader.GetString(4), AuthVersion = reader.GetInt64(5)
        } : null;
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => ExistsAsync("Username", username, 50, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => ExistsAsync("Email", email, 100, ct);

    private async Task<bool> ExistsAsync(string column, string value, int size, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand($"SELECT COUNT(1) FROM dbo.Users WHERE {column} = @Value", connection);
        command.Parameters.Add("@Value", SqlDbType.NVarChar, size).Value = value;
        return (int)(await command.ExecuteScalarAsync(ct) ?? 0) > 0;
    }

    public async Task CreateAsync(CustomerRegistration user, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.Users (Username, PasswordHash, FirstName, LastName, Email, Phone, City, Ward, Address)
            VALUES (@Username, @PasswordHash, @FirstName, @LastName, @Email, @Phone, N'Ho Chi Minh', @Ward, @Address)
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = user.Username;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = user.PasswordHash;
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = user.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = user.LastName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = user.Email;
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = user.Phone;
        command.Parameters.Add("@Ward", SqlDbType.NVarChar, 100).Value = user.Ward;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = user.Address;
        await command.ExecuteNonQueryAsync(ct);
    }
}
