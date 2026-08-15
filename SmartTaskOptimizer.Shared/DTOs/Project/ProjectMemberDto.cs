namespace SmartTaskOptimizer.Shared.DTOs.Project;

public sealed class AddProjectMemberDto
{
    public Guid UserId { get; init; }
    public string Role { get; init; } = "Member";
}

public sealed class ProjectMemberDto
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}
