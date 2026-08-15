using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.ToTable("TaskTags");
        builder.HasKey(x => new { x.TaskId, x.TagId });
        builder.HasOne(x => x.Task).WithMany(x => x.TaskTags)
            .HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Tag).WithMany(x => x.TaskTags)
            .HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
