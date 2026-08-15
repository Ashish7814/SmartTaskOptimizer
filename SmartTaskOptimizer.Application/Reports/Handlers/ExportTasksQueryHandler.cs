using MediatR;
using SmartTaskOptimizer.Application.Reports.Queries;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Reports.Service;
using SmartTaskOptimizer.Domain.Repositories.Reports;

namespace SmartTaskOptimizer.Application.Reports.Handlers;

public sealed class ExportTasksQueryHandler : IRequestHandler<ExportTasksQuery, byte[]>
{
    private readonly IReportRepository _repository;
    private readonly IExportService _exportService;
    private readonly ICurrentUserService _currentUser;
    public ExportTasksQueryHandler(IReportRepository repository, IExportService exportService, ICurrentUserService currentUser) { _repository = repository; _exportService = exportService; _currentUser = currentUser; }

    public async Task<byte[]> Handle(ExportTasksQuery request, CancellationToken cancellationToken)
    {
        var data = await _repository.GetTaskReportAsync(_currentUser.UserId, cancellationToken);
        return request.Fromat.Trim().ToLowerInvariant() switch
        {
            "excel" => _exportService.ExportExcel(data),
            "pdf" => _exportService.ExportPdf(data),
            _ => throw new ArgumentException("Supported formats are excel and pdf.")
        };
    }
}
