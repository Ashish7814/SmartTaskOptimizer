using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
