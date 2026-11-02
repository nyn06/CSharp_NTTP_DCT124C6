namespace MetropolitanCoffee.Core.Models;

public sealed record CustomerProfile(int Id, string Username, string FirstName, string LastName,
    string Email, string Phone, string City, string Ward, string Address, string Status, string? Avatar);

public sealed record CustomerProfileUpdate(string FirstName, string LastName, string Phone, string Ward, string Address);
