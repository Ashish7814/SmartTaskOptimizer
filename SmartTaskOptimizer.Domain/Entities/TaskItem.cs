using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Domain.Entities;

public sealed class TaskItem : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public Shared.Enums.TaskStatus Status { get; set; } = Shared.Enums.TaskStatus.Pending;
    public int EstimatedDurationMinutes { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Progress { get; set; }
    public string? Category { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? ProjectId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public User CreatedByUser { get; set; } = null!;
    public User? Assignee { get; set; }
    public Project? Project { get; set; }
    public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    public ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
