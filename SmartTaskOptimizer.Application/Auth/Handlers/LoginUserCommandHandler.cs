using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using Microsoft.Extensions.Configuration;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, AuthTokenResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly IConfiguration _config;

    private readonly PasswordHasher<User> _hasher = new();

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtService,
        RefreshTokenService refreshTokenService,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _config = config;
    }

    public async Task<AuthTokenResult> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var refreshTokenDays = _config.GetValue<int>("Jwt:RefreshTokenDays", 7);
        var email = request.Dto.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordResult = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Dto.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var accessToken = _jwtService.GenerateToken(user, out var expiresAtUtc);

        var rawRefreshToken = _refreshTokenService.GenerateToken();

        var refreshTokenHash = _refreshTokenService.HashToken(rawRefreshToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            CreatedByIp = request.IpAddress
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        return new AuthTokenResult
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
