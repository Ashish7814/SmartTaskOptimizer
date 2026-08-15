namespace SmartTaskOptimizer.Shared.DTOs.Auth;

public sealed class RegisterDto
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
