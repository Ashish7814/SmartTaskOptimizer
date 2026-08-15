namespace SmartTaskOptimizer.Shared.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
