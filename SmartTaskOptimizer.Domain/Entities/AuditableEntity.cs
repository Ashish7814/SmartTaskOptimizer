namespace SmartTaskOptimizer.Domain.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public Guid CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }
}
