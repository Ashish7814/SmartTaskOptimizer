using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.TaskHistory;

public sealed class TaskHistoryRepository : ITaskHistoryRepository
{
    private readonly AppDbContext _context;
    public TaskHistoryRepository(AppDbContext context) => _context = context;

    public async Task AddTaskHistoryAsync(SmartTaskOptimizer.Domain.Entities.TaskHistory taskHistory, CancellationToken cancellationToken)
    {
        await _context.TaskHistories.AddAsync(taskHistory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<SmartTaskOptimizer.Domain.Entities.TaskHistory>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) =>
        _context.TaskHistories.AsNoTracking().Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
}
