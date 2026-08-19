using MediatR;
using SmartTaskOptimizer.Application.Auth.Service;

namespace SmartTaskOptimizer.Application.Auth.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress)
    : IRequest<AuthTokenResult>;