using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Project).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Task).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.SetNull);
    }
}
