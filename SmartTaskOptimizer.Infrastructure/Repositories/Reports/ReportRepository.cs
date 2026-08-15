using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Repositories.Reports;
using SmartTaskOptimizer.Infrastructure.Data;
using SmartTaskOptimizer.Shared.DTOs.Reports;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Reports;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;
    public ReportRepository(AppDbContext context) => _context = context;

    public Task<List<TaskReportDto>> GetTaskReportAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.Tasks.AsNoTracking().Where(t => t.ProjectId.HasValue ? t.Project!.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId) || _context.Users.Any(u => u.Id == userId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) : t.CreatedByUserId == userId).OrderBy(x => x.Deadline)
            .Select(t => new TaskReportDto { Title = t.Title, Status = t.Status, Priority = t.Priority, Deadline = t.Deadline })
            .ToListAsync(cancellationToken);
}
