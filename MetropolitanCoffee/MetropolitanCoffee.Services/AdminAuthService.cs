using BCrypt.Net;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class AdminAuthService(ICustomerUserRepository users)
{
    public async Task<(CustomerUser? User, string? Error)> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return (null, "Vui lòng nhập tên đăng nhập và mật khẩu.");

        var user = await users.FindByUsernameAsync(username.Trim(), ct);
        var valid = false;
        try { valid = user is not null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash); }
        catch (SaltParseException) { }

        if (!valid || user!.Role != "admin" || user.Status != "active")
            return (null, "Tài khoản hoặc mật khẩu không đúng, hoặc tài khoản không có quyền quản trị.");
        return (user, null);
    }
}
