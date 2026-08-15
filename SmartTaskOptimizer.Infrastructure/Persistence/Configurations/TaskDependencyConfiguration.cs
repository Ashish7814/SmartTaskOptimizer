using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    public void Configure(EntityTypeBuilder<TaskDependency> builder)
    {
        builder.ToTable("TaskDependencies");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TaskId, x.DependsOnTaskId }).IsUnique();
        builder.HasOne(x => x.Task).WithMany(x => x.Dependencies)
            .HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.DependsOnTask).WithMany()
            .HasForeignKey(x => x.DependsOnTaskId).OnDelete(DeleteBehavior.Restrict);
    }
}
