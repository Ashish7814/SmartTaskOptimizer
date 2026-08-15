namespace SmartTaskOptimizer.Shared.DTOs.Tasks;

public sealed class UpdateTaskDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int? Priority { get; init; }
    public int? Status { get; init; }
    public int? EstimatedDuration { get; init; }
    public DateTime? Deadline { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Category { get; init; }
    public int? Progress { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
    public IReadOnlyList<Guid>? DependencyIds { get; init; }
    public byte[]? RowVersion { get; init; }
}
