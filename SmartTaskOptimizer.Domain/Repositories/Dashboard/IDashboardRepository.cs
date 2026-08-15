using SmartTaskOptimizer.Shared.DTOs.Dashboard;

namespace SmartTaskOptimizer.Domain.Repositories.Dashboard;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}
