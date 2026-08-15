using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Shared.DTOs.Tasks;

public sealed class TaskQueryDto
{
    public Guid? ViewerUserId { get; init; }
    public Guid? ProjectId { get; init; }
    public Enums.TaskStatus? Status { get; init; }
    public PriorityLevel? Priority { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Search { get; init; }
    public string? Category { get; init; }
    public string? Tag { get; init; }
    public bool IncludeCompleted { get; init; } = true;
    public string SortBy { get; init; } = "updatedAt";
    public bool Descending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}
