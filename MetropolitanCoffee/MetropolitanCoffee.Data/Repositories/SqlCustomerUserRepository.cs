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

    public async Task<CustomerProfile?> GetProfileAsync(int id, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            SELECT Id, Username, FirstName, LastName, Email, Phone, City, Ward, Address, Status, Avatar
            FROM dbo.Users WHERE Id=@Id AND Role=N'user' AND Status=N'active'
            """, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? new CustomerProfile(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetString(4), ReadString(reader, 5), ReadString(reader, 6), ReadString(reader, 7),
            ReadString(reader, 8), reader.GetString(9), reader.IsDBNull(10) ? null : reader.GetString(10)) : null;
    }

    public async Task<bool> UpdateAvatarAsync(int id, string avatarPath, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            UPDATE dbo.Users SET Avatar=@Avatar
            WHERE Id=@Id AND Role=N'user' AND Status=N'active'
            """, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@Avatar", SqlDbType.NVarChar, 255).Value = avatarPath;
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task<bool> UpdateProfileAsync(int id, CustomerProfileUpdate profile, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            UPDATE dbo.Users SET FirstName=@FirstName, LastName=@LastName, Phone=@Phone,
                Ward=@Ward, Address=@Address
            WHERE Id=@Id AND Role=N'user' AND Status=N'active'
            """, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = profile.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = profile.LastName;
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = profile.Phone;
        command.Parameters.Add("@Ward", SqlDbType.NVarChar, 100).Value = profile.Ward;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = profile.Address;
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task<long?> ChangePasswordAsync(int id, string previousHash, long authVersion, string newHash, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("""
            UPDATE dbo.Users SET PasswordHash=@NewHash, AuthVersion=AuthVersion+1
            OUTPUT INSERTED.AuthVersion
            WHERE Id=@Id AND PasswordHash=@PreviousHash AND AuthVersion=@AuthVersion
              AND AuthVersion<4294967295 AND Role=N'user' AND Status=N'active'
            """, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@PreviousHash", SqlDbType.NVarChar, 255).Value = previousHash;
        command.Parameters.Add("@AuthVersion", SqlDbType.BigInt).Value = authVersion;
        command.Parameters.Add("@NewHash", SqlDbType.NVarChar, 255).Value = newHash;
        return await command.ExecuteScalarAsync(ct) is long version ? version : null;
    }

    private static string ReadString(SqlDataReader reader, int index) => reader.IsDBNull(index) ? "" : reader.GetString(index);

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
