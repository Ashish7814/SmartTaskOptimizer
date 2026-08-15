using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;

public interface ITaskHistoryRepository
{
    Task AddTaskHistoryAsync(TaskHistory taskHistory, CancellationToken cancellationToken);
    Task<List<TaskHistory>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
}
