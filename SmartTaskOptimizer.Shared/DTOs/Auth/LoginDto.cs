namespace SmartTaskOptimizer.Shared.DTOs.Auth;

public sealed class LoginDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
