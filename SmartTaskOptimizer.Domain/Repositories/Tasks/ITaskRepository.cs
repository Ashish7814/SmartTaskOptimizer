using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Shared.DTOs.Common;
using SmartTaskOptimizer.Shared.DTOs.Tasks;

namespace SmartTaskOptimizer.Domain.Repositories.Tasks;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TaskItem>> GetByIdsAsync(IReadOnlyList<Guid> ids, Guid viewerUserId, CancellationToken cancellationToken);
    Task<PagedResult<TaskItem>> SearchAsync(TaskQueryDto query, CancellationToken cancellationToken);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);
    Task SoftDeleteAsync(TaskItem task, CancellationToken cancellationToken);
    Task ReplaceTagsAsync(Guid taskId, IReadOnlyList<string> tags, CancellationToken cancellationToken);
    Task ReplaceDependenciesAsync(Guid taskId, IReadOnlyList<Guid> dependencyIds, CancellationToken cancellationToken);
    Task<TaskStatistics> GetStatisticsAsync(Guid? projectId, Guid viewerUserId, CancellationToken cancellationToken);
}
