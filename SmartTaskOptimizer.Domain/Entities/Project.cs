namespace SmartTaskOptimizer.Domain.Entities;

public sealed class Project : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
