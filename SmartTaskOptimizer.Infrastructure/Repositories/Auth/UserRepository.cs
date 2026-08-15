using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Auth;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Users.SingleOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public async Task UpdateLastLoginAsync(Guid userId, DateTime lastLoginAt, CancellationToken cancellationToken)
    {
        await _context.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastLoginAt, lastLoginAt), cancellationToken);
    }

    public async Task UpdateProfileAsync(Guid userId, string fullName, CancellationToken cancellationToken)
    {
        await _context.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.FullName, fullName).SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken);
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken)
    {
        await _context.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.PasswordHash, passwordHash).SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken);
    }
}
