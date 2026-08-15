using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Comments;

public interface ITaskCommentRepository
{
    Task AddAsync(TaskComment comment, CancellationToken cancellationToken);
    Task<List<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskComment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
