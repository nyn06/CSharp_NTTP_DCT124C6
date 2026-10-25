namespace MetropolitanCoffee.Core.Models;

public sealed class CustomerRegistration
{
    public string Username { get; init; } = "";
    public string Email { get; init; } = "";
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Ward { get; init; } = "";
    public string Address { get; init; } = "";
    public string PasswordHash { get; init; } = "";
}
