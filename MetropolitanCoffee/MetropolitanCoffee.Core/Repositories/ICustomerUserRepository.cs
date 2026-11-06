using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface ICustomerUserRepository
{
    Task<CustomerUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<CustomerUser?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetProfileAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int id, CustomerProfileUpdate profile, CancellationToken cancellationToken = default);
    Task<bool> UpdateAvatarAsync(int id, string avatarPath, CancellationToken cancellationToken = default);
    Task<long?> ChangePasswordAsync(int id, string previousHash, long authVersion, string newHash, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task CreateAsync(CustomerRegistration registration, CancellationToken cancellationToken = default);
}
