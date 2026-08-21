using MediatR;

using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using Microsoft.Extensions.Configuration;
using SmartTaskOptimizer.Domain.Repositories.Auth;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokenResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly IConfiguration _config;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtService,
        RefreshTokenService refreshTokenService,
        IConfiguration config)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _config = config;
    }

    public async Task<AuthTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        /*
         * Hash the raw refresh token received
         * from the HttpOnly cookie.
         */
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        /*
         * Find the refresh token by its hash.
         */
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        /*
         * Token does not exist.
         */
        if (storedToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }
        /*
         * Detect refresh-token reuse.
         *
         * If a revoked token is presented again,
         * revoke all sessions for this user.
         */
        if (storedToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId, request.IpAddress, cancellationToken);
            throw new UnauthorizedAccessException(
                "Refresh token has been revoked.");
        }
        /*
         * Refresh token has expired.
         */
        if (storedToken.IsExpired)
        {
            await _refreshTokenRepository.RevokeAsync(storedToken, request.IpAddress, null, cancellationToken); 
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }
        /*
         * Get the user associated with
         * the refresh token.
         */
        var user = storedToken.User;
        /*
         * Do not issue tokens to inactive users.
         */
        if (!user.IsActive)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, request.IpAddress, cancellationToken);
            throw new UnauthorizedAccessException("User account is inactive.");
        }
        /*
         * Generate a new short-lived access token.
         */
        var newAccessToken = _jwtService.GenerateToken(user, out var expiresAtUtc);
        /*
         * Generate a completely new refresh token.
         */
        var newRawRefreshToken = _refreshTokenService.GenerateToken();
        /*
         * Store only the hash of the new
         * refresh token.
         */
        var newRefreshTokenHash = _refreshTokenService.HashToken(newRawRefreshToken);
        /*
         * Refresh tokens currently have a
         * 7-day lifetime.
         *
         * This replaces the IConfiguration/
         * GetValue dependency so the Application
         * project remains independent of the
         * ASP.NET configuration binder.
         */
       var refreshTokenDays = _config["Jwt:RefreshTokenDays"];

        /*
         * Create replacement refresh token.
         */
        var newRefreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedByIp = request.IpAddress
            };
        /*
         * Revoke the old refresh token.
         */
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;
        /*
         * Link old token to replacement token.
         */
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;
        /*
         * Atomically rotate:
         *
         * OLD TOKEN
         *     ↓
         * REVOKED
         *
         * NEW TOKEN
         *     ↓
         * ACTIVE
         */
        await _refreshTokenRepository.RotateAsync(storedToken, newRefreshToken, cancellationToken);
        /*
         * Return the new access token and
         * raw refresh token.
         *
         * AuthController will put the raw
         * refresh token into the HttpOnly cookie.
         */
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
