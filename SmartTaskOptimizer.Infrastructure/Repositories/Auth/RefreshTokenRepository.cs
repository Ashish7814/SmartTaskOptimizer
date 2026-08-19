using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Auth;

public sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return _context.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task RotateAsync(
        RefreshToken oldToken,
        RefreshToken newToken,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        _context.RefreshTokens.Update(oldToken);

        await _context.RefreshTokens.AddAsync(
            newToken,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);
    }

    public async Task RevokeAsync(
        RefreshToken refreshToken,
        string? ipAddress,
        string? replacedByTokenHash,
        CancellationToken cancellationToken)
    {
        refreshToken.RevokedAt =
            DateTime.UtcNow;

        refreshToken.RevokedByIp =
            ipAddress;

        refreshToken.ReplacedByTokenHash =
            replacedByTokenHash;

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var tokens =
            await _context.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    x.RevokedAt == null)
                .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.RevokedByIp = ipAddress;
        }

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
