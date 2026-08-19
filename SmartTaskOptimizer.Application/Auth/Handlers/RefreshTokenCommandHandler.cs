using MediatR;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Domain.Repositories.Auth;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthTokenResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RefreshTokenService _refreshTokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        RefreshTokenService refreshTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthTokenResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedAccessException(
                "Refresh token is required.");
        }

        return await _refreshTokenService.RefreshAsync(
            request.RefreshToken,
            cancellationToken);
    }
}
