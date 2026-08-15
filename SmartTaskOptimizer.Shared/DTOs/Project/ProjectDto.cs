namespace SmartTaskOptimizer.Shared.DTOs.Project;

public sealed class ProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid OwnerId { get; init; }
    public int MemberCount { get; init; }
    public int TaskCount { get; init; }
}
