namespace SmartTaskOptimizer.Domain.Repositories.BackgroundJobs;

public interface ITaskBackgroundJob
{
    Task RecalculatePrioritiesAsync();
}
