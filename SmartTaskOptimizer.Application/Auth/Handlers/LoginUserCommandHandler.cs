using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Shared.DTOs.Auth;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _repository;
    private readonly IJwtTokenService _jwtService;
    private readonly PasswordHasher<Domain.Entities.User> _hasher = new();

    public LoginUserCommandHandler(IUserRepository repository, IJwtTokenService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Dto.Email.Trim().ToLowerInvariant();
        var user = await _repository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var lastLoginAt = DateTime.UtcNow;
        await _repository.UpdateLastLoginAsync(user.Id, lastLoginAt, cancellationToken);
        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60)
        };
    }
}
