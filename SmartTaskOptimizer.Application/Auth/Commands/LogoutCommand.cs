using MediatR;

namespace SmartTaskOptimizer.Application.Auth.Commands;

public sealed record LogoutCommand(
    string RefreshToken,
    string? IpAddress)
    : IRequest;