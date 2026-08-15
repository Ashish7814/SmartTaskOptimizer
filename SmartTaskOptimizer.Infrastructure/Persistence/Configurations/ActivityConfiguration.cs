using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Field).HasMaxLength(100);
        builder.Property(x => x.OldValue).HasMaxLength(2000);
        builder.Property(x => x.NewValue).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ProjectId, x.CreatedAt });
        builder.HasOne(x => x.Project).WithMany(x => x.Activities)
            .HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Actor).WithMany(x => x.Activities)
            .HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Task).WithMany(x => x.Activities)
            .HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.SetNull);
    }
}
