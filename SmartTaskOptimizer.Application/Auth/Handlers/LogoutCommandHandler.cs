using MediatR;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Domain.Repositories.Auth;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class LogoutCommandHandler
    : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _repository;
    private readonly RefreshTokenService _tokenService;

    public LogoutCommandHandler(
        IRefreshTokenRepository repository,
        RefreshTokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                request.RefreshToken))
        {
            return;
        }

        var hash =
            _tokenService.HashToken(
                request.RefreshToken);

        var token =
            await _repository.GetByTokenHashAsync(
                hash,
                cancellationToken);

        if (token is null || token.IsRevoked)
        {
            return;
        }

        await _repository.RevokeAsync(
            token,
            request.IpAddress,
            null,
            cancellationToken);
    }
}