using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.ToTable("TaskHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OldStatus).HasConversion<int>();
        builder.Property(x => x.NewStatus).HasConversion<int>();
        builder.Property(x => x.OldPriority).HasConversion<int>();
        builder.Property(x => x.NewPriority).HasConversion<int>();
        builder.Property(x => x.ChangeReason).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TaskId, x.CreatedAt });
        builder.HasOne(x => x.ChangedByUser).WithMany(x => x.TaskHistories)
            .HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
