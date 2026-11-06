
using System.Net.Mail;
using MetropolitanCoffee.Data.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class CustomerManagementService
{
    private readonly SqlCustomerManagementRepository _repository;

    public CustomerManagementService(
        SqlCustomerManagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<(bool Success, string Message)>
        AddCustomerAsync(
            string username,
            string email,
            string password,
            string confirmPassword)
    {
        username = username.Trim();
        email = email.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            return (false, "Vui lòng nhập đầy đủ thông tin.");
        }

        if (username.Length > 50)
            return (false, "Tên đăng nhập tối đa 50 ký tự.");

        if (email.Length > 100 || !IsValidEmail(email))
            return (false, "Email không hợp lệ.");

        if (password.Length < 6)
            return (false, "Mật khẩu phải có ít nhất 6 ký tự.");

        if (password != confirmPassword)
            return (false, "Mật khẩu xác nhận không khớp.");

        if (await _repository.UsernameExistsAsync(username))
            return (false, "Tên đăng nhập đã tồn tại.");

        if (await _repository.EmailExistsAsync(email))
            return (false, "Email đã được sử dụng.");

        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        try
        {
            bool result = await _repository.AddCustomerAsync(
                username, email, hash);

            return result
                ? (true, "Thêm khách hàng thành công.")
                : (false, "Không thể thêm khách hàng.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
            when (ex.Number is 2601 or 2627)
        {
            return (false, "Tên đăng nhập hoặc email đã tồn tại.");
        }
    }

    public async Task<(bool Success, string Message)>
        LockCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return (false, "Khách hàng không hợp lệ.");

        bool result = await _repository
            .SetCustomerStatusAsync(customerId, true);

        return result
            ? (true, "Khóa tài khoản thành công.")
            : (false, "Không thể khóa tài khoản.");
    }

    public async Task<(bool Success, string Message)>
        UnlockCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return (false, "Khách hàng không hợp lệ.");

        bool result = await _repository
            .SetCustomerStatusAsync(customerId, false);

        return result
            ? (true, "Mở khóa tài khoản thành công.")
            : (false, "Không thể mở khóa tài khoản.");
    }

    public async Task<(bool Success, string Message)>
        ResetPasswordAsync(int customerId)
    {
        if (customerId <= 0)
            return (false, "Khách hàng không hợp lệ.");

        const string defaultPassword = "123456";

        string hash = BCrypt.Net.BCrypt.HashPassword(
            defaultPassword);

        bool result = await _repository
            .ResetPasswordAsync(customerId, hash);

        return result
            ? (true, "Đặt lại mật khẩu thành 123456 thành công.")
            : (false, "Không thể đặt lại mật khẩu.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
