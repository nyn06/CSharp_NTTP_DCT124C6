
using System.Data;
using Microsoft.Data.SqlClient;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlCustomerManagementRepository
{
    private readonly string _connectionString;

    public SqlCustomerManagementRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DataTable> GetCustomersAsync()
    {
        const string sql = """
            SELECT
                Id AS [User ID],
                Username,
                Email,
                CASE
                    WHEN Status = N'active' THEN 'Active'
                    ELSE 'Locked'
                END AS Status,
                Role
            FROM dbo.Users
            WHERE Role = N'user'
            ORDER BY Id DESC
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        var table = new DataTable();
        table.Load(reader);

        return table;
    }

    public async Task<bool> UsernameExistsAsync(
        string username)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Users
            WHERE Username = @Username
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username", SqlDbType.NVarChar, 50)
            .Value = username;

        return Convert.ToInt32(
            await command.ExecuteScalarAsync()) > 0;
    }

    public async Task<bool> EmailExistsAsync(
        string email)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Users
            WHERE Email = @Email
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Email", SqlDbType.NVarChar, 100)
            .Value = email;

        return Convert.ToInt32(
            await command.ExecuteScalarAsync()) > 0;
    }

    public async Task<bool> AddCustomerAsync(
        string username,
        string email,
        string passwordHash)
    {
        const string sql = """
            INSERT INTO dbo.Users
                (Username, Email, PasswordHash,
                 FirstName, LastName, City, Role, Status)
            VALUES
                (@Username, @Email, @PasswordHash,
                 N'', N'', N'Ho Chi Minh',
                 N'user', N'active')
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username", SqlDbType.NVarChar, 50)
            .Value = username;

        command.Parameters.Add(
            "@Email", SqlDbType.NVarChar, 100)
            .Value = email;

        command.Parameters.Add(
            "@PasswordHash", SqlDbType.NVarChar, 255)
            .Value = passwordHash;

        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> SetCustomerStatusAsync(
        int customerId, bool locked)
    {
        const string sql = """
            UPDATE dbo.Users
            SET Status = @Status,
                AuthVersion = AuthVersion + 1
            WHERE Id = @Id
              AND Role = N'user'
              AND AuthVersion < 4294967295
              AND Status <> @Status
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Id", SqlDbType.Int)
            .Value = customerId;

        command.Parameters.Add(
            "@Status", SqlDbType.NVarChar, 20)
            .Value = locked ? "locked" : "active";

        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> ResetPasswordAsync(
        int customerId, string passwordHash)
    {
        const string sql = """
            UPDATE dbo.Users
            SET PasswordHash = @PasswordHash,
                AuthVersion = AuthVersion + 1
            WHERE Id = @Id
              AND Role = N'user'
              AND AuthVersion < 4294967295
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Id", SqlDbType.Int)
            .Value = customerId;

        command.Parameters.Add(
            "@PasswordHash", SqlDbType.NVarChar, 255)
            .Value = passwordHash;

        return await command.ExecuteNonQueryAsync() == 1;
    }
}
