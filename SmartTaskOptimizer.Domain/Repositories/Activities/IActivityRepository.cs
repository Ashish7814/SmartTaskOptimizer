using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Activities;

public interface IActivityRepository
{
    Task AddAsync(Activity activity, CancellationToken cancellationToken);
    Task<List<Activity>> GetByProjectAsync(Guid projectId, int page, int pageSize, CancellationToken cancellationToken);
}
