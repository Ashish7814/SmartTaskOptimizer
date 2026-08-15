using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Auth.Handlers;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _repository;
    private readonly PasswordHasher<User> _hasher = new();

    public RegisterUserCommandHandler(IUserRepository repository) => _repository = repository;

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.dto.Email.Trim().ToLowerInvariant();
        if (await _repository.GetByEmailAsync(email, cancellationToken) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.dto.FullName.Trim(),
            Email = email,
            Role = UserRole.User
        };
        user.PasswordHash = _hasher.HashPassword(user, request.dto.Password);
        try { await _repository.AddUserAsync(user, cancellationToken); }
        catch (DbUpdateException) { throw new ConflictException("An account with this email already exists."); }
        return user.Id;
    }
}
