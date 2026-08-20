using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task RotateAsync(
        RefreshToken oldToken,
        RefreshToken newToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        RefreshToken refreshToken,
        string? ipAddress = null,
        string? replacedByTokenHash = null,
        CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(
        Guid userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
