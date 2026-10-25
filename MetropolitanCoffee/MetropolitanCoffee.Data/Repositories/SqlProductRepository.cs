using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Data.Repositories;

public sealed class SqlProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public SqlProductRepository(string connectionString) => _connectionString = connectionString;

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT p.Id, p.CategoryId, c.Name, p.Name, p.ShortDescription,
                   p.Description, p.Ingredients, p.Image, p.SellPrice,
                   p.Quantity, p.Status, p.HasSize, p.HasIce, p.HasSugar, p.CreatedAt
            FROM dbo.Products AS p
            INNER JOIN dbo.Categories AS c ON c.Id = p.CategoryId
            WHERE p.Status = N'active'
            ORDER BY c.Name ASC, p.Name ASC, p.Id DESC
            """;

        var products = new List<Product>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(ReadProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT p.Id, p.CategoryId, c.Name, p.Name, p.ShortDescription,
                   p.Description, p.Ingredients, p.Image, p.SellPrice,
                   p.Quantity, p.Status, p.HasSize, p.HasIce, p.HasSugar, p.CreatedAt
            FROM dbo.Products AS p
            INNER JOIN dbo.Categories AS c ON c.Id = p.CategoryId
            WHERE p.Id = @Id AND p.Status = N'active'
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadProduct(reader) : null;
    }

    private static Product ReadProduct(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        CategoryId = reader.GetInt32(1),
        CategoryName = reader.GetString(2),
        Name = reader.GetString(3),
        ShortDescription = reader.IsDBNull(4) ? null : reader.GetString(4),
        Description = reader.IsDBNull(5) ? null : reader.GetString(5),
        Ingredients = reader.IsDBNull(6) ? null : reader.GetString(6),
        Image = reader.IsDBNull(7) ? null : reader.GetString(7),
        SellPrice = reader.GetDecimal(8),
        Quantity = reader.GetInt32(9),
        Status = reader.GetString(10),
        HasSize = reader.GetBoolean(11),
        HasIce = reader.GetBoolean(12),
        HasSugar = reader.GetBoolean(13),
        CreatedAt = reader.GetDateTime(14)
    };
}
