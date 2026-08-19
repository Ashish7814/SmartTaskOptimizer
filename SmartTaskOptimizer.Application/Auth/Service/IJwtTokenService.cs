using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Application.Auth.Service;

public interface IJwtTokenService
{
    string GenerateToken(
        User user,
        out DateTime expiresAtUtc);
}
