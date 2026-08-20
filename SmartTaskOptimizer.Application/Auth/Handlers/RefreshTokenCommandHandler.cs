using MediatR;

using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using Microsoft.Extensions.Configuration;

using SmartTaskOptimizer.Domain.Repositories.Auth;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<
        RefreshTokenCommand,
        AuthTokenResult>
{
    private readonly IRefreshTokenRepository
        _refreshTokenRepository;

    private readonly IJwtTokenService
        _jwtService;

    private readonly RefreshTokenService
        _refreshTokenService;

    private readonly IConfiguration
        _configuration;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtService,
        RefreshTokenService refreshTokenService,
        IConfiguration configuration)
    {
        _refreshTokenRepository =
            refreshTokenRepository;

        _jwtService =
            jwtService;

        _refreshTokenService =
            refreshTokenService;

        _configuration =
            configuration;
    }

    public async Task<AuthTokenResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        /*
         * Hash the raw refresh token received
         * from the HttpOnly cookie.
         */
        var tokenHash =
            _refreshTokenService.HashToken(
                request.RefreshToken);

        /*
         * Find the refresh token by its hash.
         */
        var storedToken =
            await _refreshTokenRepository
                .GetByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        /*
         * Token does not exist.
         */
        if (storedToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        /*
         * A revoked token is being reused.
         *
         * This can indicate that the refresh token
         * was stolen.
         *
         * Revoke all sessions for this user.
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

        /*
         * Refresh token has expired.
         */
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

        /*
         * Get user associated with refresh token.
         */
        var user =
            storedToken.User;

        /*
         * Do not issue tokens to inactive users.
         */
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

        /*
         * Generate a new short-lived access token.
         */
        var newAccessToken =
            _jwtService.GenerateToken(
                user,
                out var expiresAtUtc);

        /*
         * Generate a completely new refresh token.
         */
        var newRawRefreshToken =
            _refreshTokenService.GenerateToken();

        /*
         * Store only the hash of the new
         * refresh token.
         */
        var newRefreshTokenHash =
            _refreshTokenService.HashToken(
                newRawRefreshToken);

        /*
         * Read refresh-token lifetime from
         * configuration.
         *
         * Example:
         *
         * "RefreshTokenDays": 7
         */
        var refreshTokenDays =
            _configuration.GetValue(
                "Jwt:RefreshTokenDays",
                7);

        /*
         * Create replacement refresh token.
         */
        var newRefreshToken =
            new Domain.Entities.RefreshToken
            {
                Id =
                    Guid.NewGuid(),

                UserId =
                    user.Id,

                TokenHash =
                    newRefreshTokenHash,

                CreatedAt =
                    DateTime.UtcNow,

                ExpiresAt =
                    DateTime.UtcNow.AddDays(
                        refreshTokenDays),

                CreatedByIp =
                    request.IpAddress
            };

        /*
         * Revoke old refresh token.
         */
        storedToken.RevokedAt =
            DateTime.UtcNow;

        storedToken.RevokedByIp =
            request.IpAddress;

        /*
         * Link old token to replacement token.
         */
        storedToken.ReplacedByTokenHash =
            newRefreshTokenHash;

        /*
         * Rotate both records atomically.
         *
         * Old token:
         *     revoked
         *
         * New token:
         *     active
         */
        await _refreshTokenRepository
            .RotateAsync(
                storedToken,
                newRefreshToken,
                cancellationToken);

        /*
         * Return the new access token and
         * new refresh token to AuthController.
         *
         * AuthController sends the refresh token
         * only as an HttpOnly cookie.
         */
        return new AuthTokenResult
        {
            AccessToken =
                newAccessToken,

            RefreshToken =
                newRawRefreshToken,

            ExpiresAtUtc =
                expiresAtUtc,

            UserId =
                user.Id,

            FullName =
                user.FullName,

            Email =
                user.Email,

            Role =
                user.Role.ToString()
        };
    }
}
