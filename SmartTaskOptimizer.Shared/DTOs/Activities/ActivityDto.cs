namespace SmartTaskOptimizer.Shared.DTOs.Activities;

public sealed class ActivityDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid ActorId { get; init; }
    public string ActorName { get; init; } = string.Empty;
    public Guid? TaskId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? Field { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime CreatedAt { get; init; }
}
