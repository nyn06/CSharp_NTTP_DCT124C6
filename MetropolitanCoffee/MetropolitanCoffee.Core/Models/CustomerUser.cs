namespace MetropolitanCoffee.Core.Models;

public sealed class CustomerUser
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public string Role { get; init; } = "";
    public string Status { get; init; } = "";
    public long AuthVersion { get; init; }
}
