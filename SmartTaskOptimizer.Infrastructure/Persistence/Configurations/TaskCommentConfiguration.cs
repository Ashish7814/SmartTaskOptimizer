using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("TaskComments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Body).IsRequired().HasMaxLength(10000);
        builder.HasIndex(x => new { x.TaskId, x.CreatedAt });
    }
}
