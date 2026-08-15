using SmartTaskOptimizer.Shared.DTOs.Reports;

namespace SmartTaskOptimizer.Domain.Repositories.Reports;

public interface IReportRepository
{
    Task<List<TaskReportDto>> GetTaskReportAsync(Guid userId, CancellationToken cancellationToken = default);
}
