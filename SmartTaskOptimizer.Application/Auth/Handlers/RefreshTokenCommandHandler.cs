using MediatR;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Domain.Repositories.Auth;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<
        RefreshTokenCommand,
        AuthTokenResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly RefreshTokenService _refreshTokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtService,
        RefreshTokenService refreshTokenService)
    {
        _refreshTokenRepository =
            refreshTokenRepository;

        _jwtService = jwtService;

        _refreshTokenService =
            refreshTokenService;
    }

    public async Task<AuthTokenResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _refreshTokenService.HashToken(
                request.RefreshToken);

        var storedToken =
            await _refreshTokenRepository
                .GetByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        if (storedToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        /*
         * A revoked refresh token being presented again
         * can indicate token theft/reuse.
         *
         * Revoke all sessions for that user.
         */
        if (storedToken.IsRevoked)
        {
            await _refreshTokenRepository
                .RevokeAllForUserAsync(
                    storedToken.UserId,
                    request.IpAddress,
                    cancellationToken);

            throw new UnauthorizedAccessException(
                "Refresh token has been revoked.");
        }

        if (storedToken.IsExpired)
        {
            await _refreshTokenRepository
                .RevokeAsync(
                    storedToken,
                    request.IpAddress,
                    null,
                    cancellationToken);

            throw new UnauthorizedAccessException(
                "Refresh token has expired.");
        }

        var user = storedToken.User;

        if (!user.IsActive)
        {
            await _refreshTokenRepository
                .RevokeAllForUserAsync(
                    user.Id,
                    request.IpAddress,
                    cancellationToken);

            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        var newAccessToken =
            _jwtService.GenerateToken(
                user,
                out var expiresAtUtc);

        var newRawRefreshToken =
            _refreshTokenService.GenerateToken();

        var newRefreshTokenHash =
            _refreshTokenService.HashToken(
                newRawRefreshToken);

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt =
                DateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress
        };

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;
        storedToken.ReplacedByTokenHash =
            newRefreshTokenHash;

        /*
         * The repository needs to save both records atomically.
         */
        await _refreshTokenRepository
            .RotateAsync(
                storedToken,
                newRefreshToken,
                cancellationToken);

        return new AuthTokenResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}