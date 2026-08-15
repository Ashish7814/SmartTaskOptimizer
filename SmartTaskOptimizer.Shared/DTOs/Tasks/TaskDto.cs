using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Shared.DTOs.Tasks;

public sealed class TaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public PriorityLevel Priority { get; init; }
    public Enums.TaskStatus Status { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public DateTime Deadline { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string? CreatedByName { get; init; }
    public string? Category { get; init; }
    public int Progress { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<Guid> DependencyIds { get; init; } = Array.Empty<Guid>();
    public byte[]? RowVersion { get; init; }

    // Backward-compatible aliases retained for existing clients.
    public int EstimatedDuration => EstimatedDurationMinutes;
    public DateTime DueDate => Deadline;
    public IReadOnlyList<string> Dependencies => DependencyIds.Select(x => x.ToString()).ToArray();
}

public sealed class OptimizeTasksRequest
{
    public IReadOnlyList<Guid> TaskIds { get; init; } = Array.Empty<Guid>();
}

public sealed class OptimizationResult
{
    public IReadOnlyList<ScheduledTask> OptimizedSchedule { get; init; } = Array.Empty<ScheduledTask>();
    public int TotalDuration { get; init; }
    public IReadOnlyList<string> Suggestions { get; init; } = Array.Empty<string>();
    public double Efficiency { get; init; }
}

public sealed class ScheduledTask
{
    public TaskDto Task { get; init; } = new();
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int Order { get; init; }
}

public sealed class TaskStatistics
{
    public int Total { get; init; }
    public int Completed { get; init; }
    public int InProgress { get; init; }
    public int Pending { get; init; }
    public int Overdue { get; init; }
    public double CompletionRate { get; init; }
    public int AverageDurationMinutes { get; init; }
    public IReadOnlyDictionary<PriorityLevel, int> ByPriority { get; init; } = new Dictionary<PriorityLevel, int>();
    public IReadOnlyDictionary<Enums.TaskStatus, int> ByStatus { get; init; } = new Dictionary<Enums.TaskStatus, int>();
}
