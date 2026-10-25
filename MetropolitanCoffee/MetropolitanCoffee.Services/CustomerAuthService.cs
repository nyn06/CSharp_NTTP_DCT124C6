using BCrypt.Net;
using Microsoft.Data.SqlClient;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class CustomerAuthService(ICustomerUserRepository users)
{
    public async Task<(CustomerUser? User, string? Error)> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (null, "Please enter both username and password!");

        var user = await users.FindByUsernameAsync(username.Trim(), ct);
        bool valid;
        try { valid = user is not null && BCrypt.Net.BCrypt.Verify(password.Trim(), user.PasswordHash); }
        catch (SaltParseException) { valid = false; }
        if (!valid || user!.Role != "user")
            return (null, "Login failed. Please check your username and password");
        if (user.Status == "locked")
            return (null, "Your account has been locked. Please contact admin by email: metropolitancoffee@gmail.com.");
        return (user, null);
    }

    public async Task<string?> RegisterAsync(string username, string email, string firstName, string lastName,
        string phone, string ward, string address, string password, string confirmPassword, CancellationToken ct = default)
    {
        username = username.Trim(); email = email.Trim().ToLowerInvariant(); firstName = firstName.Trim();
        lastName = lastName.Trim(); phone = phone.Trim(); ward = ward.Trim(); address = address.Trim();
        if (new[] { username, email, firstName, lastName, phone, ward, address, password, confirmPassword }.Any(string.IsNullOrWhiteSpace))
            return "Please fill in all required fields.";
        if (username.Length > 50 || !System.Text.RegularExpressions.Regex.IsMatch(username, @"^[\p{L}0-9\s]+$"))
            return "Username can only contain letters, numbers, and spaces (up to 50 characters).";
        if (firstName.Length > 50 || lastName.Length > 50 ||
            !System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[\p{L}\s]+$") ||
            !System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[\p{L}\s]+$"))
            return "Names can only contain letters and spaces (up to 50 characters).";
        if (email.Length > 100 || !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@gmail\.com$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "Email must be a valid Gmail address.";
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0[0-9]{9}$"))
            return "Phone number must start with 0 and contain 10 digits.";
        if (ward.Length > 100 || !System.Text.RegularExpressions.Regex.IsMatch(ward, @"^[A-Za-z\s]+$"))
            return "Please select a valid ward from the list.";
        if (address.Length > 255 || !System.Text.RegularExpressions.Regex.IsMatch(address, @"^[\p{L}0-9\s/\-]+$"))
            return "Address can only contain letters, numbers, spaces, / and -.";
        if (System.Text.Encoding.UTF8.GetByteCount(password) < 6)
            return "Password must be at least 6 characters long.";
        if (password != confirmPassword)
            return "Passwords do not match.";
        if (await users.UsernameExistsAsync(username, ct)) return "This username is already taken.";
        if (await users.EmailExistsAsync(email, ct)) return "This email is already registered.";

        try
        {
            await users.CreateAsync(new CustomerRegistration
            {
                Username = username, Email = email, FirstName = firstName, LastName = lastName,
                Phone = phone, Ward = ward, Address = address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            }, ct);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            if (await users.UsernameExistsAsync(username, ct)) return "This username is already taken.";
            return "This email is already registered.";
        }
        return null;
    }
}
