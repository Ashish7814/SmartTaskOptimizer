using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Auth;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateLastLoginAsync(Guid userId, DateTime lastLoginAt, CancellationToken cancellationToken);
    Task UpdateProfileAsync(Guid userId, string fullName, CancellationToken cancellationToken);
    Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken);
}
