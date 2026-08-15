namespace SmartTaskOptimizer.Domain.Entities;

public sealed class TaskComment : BaseEntity
{
    public Guid TaskId { get; set; }
    public Guid AuthorId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public TaskItem Task { get; set; } = null!;
    public User Author { get; set; } = null!;
}
