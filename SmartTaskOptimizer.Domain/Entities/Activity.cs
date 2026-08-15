namespace SmartTaskOptimizer.Domain.Entities;

public sealed class Activity : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid ActorId { get; set; }
    public Guid? TaskId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public Project Project { get; set; } = null!;
    public User Actor { get; set; } = null!;
    public TaskItem? Task { get; set; }
}
