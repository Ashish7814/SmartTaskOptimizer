using MediatR;
using SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;
using SmartTaskOptimizer.Application.TaskHistory.Queries;
using SmartTaskOptimizer.Shared.DTOs.TaskHistory;

namespace SmartTaskOptimizer.Application.TaskHistory.Handler;

public sealed class GetTaskHistoryQueryHandler : IRequestHandler<GetTaskHistoryQuery, List<TaskHistoryDto>>
{
    private readonly ITaskHistoryRepository _repository;
    public GetTaskHistoryQueryHandler(ITaskHistoryRepository repository) => _repository = repository;
    public async Task<List<TaskHistoryDto>> Handle(GetTaskHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _repository.GetByTaskIdAsync(request.taskId, cancellationToken);
        return history.Select(h => new TaskHistoryDto { OldStatus = h.OldStatus, NewStatus = h.NewStatus, OldPriority = h.OldPriority, NewPriority = h.NewPriority, ChangedAt = h.CreatedAt, ChangedByUserId = h.ChangedByUserId }).ToList();
    }
}
