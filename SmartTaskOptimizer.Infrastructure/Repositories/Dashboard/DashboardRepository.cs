using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Repositories.Dashboard;
using SmartTaskOptimizer.Infrastructure.Data;
using SmartTaskOptimizer.Shared.DTOs.Dashboard;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Dashboard;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;
    public DashboardRepository(AppDbContext context) => _context = context;

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = _context.Tasks.AsNoTracking().Where(t => t.ProjectId.HasValue ? t.Project!.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId) || _context.Users.Any(u => u.Id == userId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) : t.CreatedByUserId == userId);
        return new DashboardStatsDto
        {
            TotalTasks = await tasks.CountAsync(cancellationToken),
            PendingTasks = await tasks.CountAsync(t => t.Status == Shared.Enums.TaskStatus.Pending, cancellationToken),
            InProgressTasks = await tasks.CountAsync(t => t.Status == Shared.Enums.TaskStatus.InProgress, cancellationToken),
            CompletedTasks = await tasks.CountAsync(t => t.Status == Shared.Enums.TaskStatus.Completed, cancellationToken),
            CancelledTasks = await tasks.CountAsync(t => t.Status == Shared.Enums.TaskStatus.Cancelled, cancellationToken),
            OnHoldTasks = await tasks.CountAsync(t => t.Status == Shared.Enums.TaskStatus.OnHold, cancellationToken),
            HighPriorityTasks = await tasks.CountAsync(t => t.Priority == PriorityLevel.High, cancellationToken),
            CriticalPriorityTasks = await tasks.CountAsync(t => t.Priority == PriorityLevel.Critical, cancellationToken),
            OverdueTasks = await tasks.CountAsync(t => t.Deadline < DateTime.UtcNow && t.Status != Shared.Enums.TaskStatus.Completed && t.Status != Shared.Enums.TaskStatus.Cancelled, cancellationToken)
        };
    }
}
