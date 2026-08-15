using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(5000);
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Priority).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.Progress).HasDefaultValue(0).IsRequired();
        builder.HasIndex(x => new { x.ProjectId, x.Status });
        builder.HasIndex(x => new { x.ProjectId, x.UpdatedAt });
        builder.HasIndex(x => new { x.ProjectId, x.Deadline });
        builder.HasIndex(x => x.AssigneeId);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Project).WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CreatedByUser).WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Assignee).WithMany(x => x.AssignedTasks)
            .HasForeignKey(x => x.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.History).WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Comments).WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}
