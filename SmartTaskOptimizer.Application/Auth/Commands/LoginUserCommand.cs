using MediatR;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Shared.DTOs.Auth;

namespace SmartTaskOptimizer.Application.Auth.Commands;

public sealed record LoginUserCommand(
    LoginDto Dto,
    string? IpAddress)
    : IRequest<AuthTokenResult>;
