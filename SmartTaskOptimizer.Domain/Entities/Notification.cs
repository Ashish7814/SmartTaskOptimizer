namespace SmartTaskOptimizer.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
    public Project? Project { get; set; }
    public TaskItem? Task { get; set; }
}
