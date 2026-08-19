using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        RefreshToken refreshToken,
        string? ipAddress,
        string? replacedByTokenHash,
        CancellationToken cancellationToken);

    Task RevokeAllForUserAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken);
}
