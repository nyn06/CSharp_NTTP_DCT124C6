using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Core.Repositories;

public interface ICustomerUserRepository
{
    Task<CustomerUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<CustomerUser?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task CreateAsync(CustomerRegistration registration, CancellationToken cancellationToken = default);
}
