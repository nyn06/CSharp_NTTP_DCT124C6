namespace MetropolitanCoffee.Core.Models;

// dbo.Categories: Id INT, Name NVARCHAR(100), CreatedAt DATETIME2(0).
public sealed class Category
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
