using System.ComponentModel.DataAnnotations;

namespace SmartTaskOptimizer.Shared.DTOs.Users;

public sealed class UpdateProfileDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;
}

public sealed class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; init; } = string.Empty;
}

public sealed class UserProfileDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
