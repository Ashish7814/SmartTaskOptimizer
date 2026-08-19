using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartTaskOptimizer.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTaskOptimizer.Application.Auth.Service;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(
        User user,
        out DateTime expiresAtUtc)
    {
        var keyValue = _config["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(keyValue) ||
            Encoding.UTF8.GetByteCount(keyValue) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be configured with at least 256 bits of entropy.");
        }

        var expirationMinutes = 10;

        var configuredExpiration =
            _config["Jwt:AccessTokenMinutes"];

        if (!string.IsNullOrWhiteSpace(configuredExpiration) &&
            int.TryParse(configuredExpiration, out var parsed) &&
            parsed > 0)
        {
            expirationMinutes = parsed;
        }

        var now = DateTime.UtcNow;

        expiresAtUtc =
            now.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(keyValue));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
