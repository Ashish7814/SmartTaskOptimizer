namespace SmartTaskOptimizer.Shared.DTOs.Tasks;

public sealed class CreateTaskDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Priority { get; init; } = 2;
    public int EstimatedDuration { get; init; }
    public DateTime Deadline { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Category { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<Guid> DependencyIds { get; init; } = Array.Empty<Guid>();
}
