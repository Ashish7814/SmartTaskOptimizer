using SmartTaskOptimizer.Application.Priorities;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.BackgroundJobs;
using SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Shared.DTOs.Tasks;

namespace SmartTaskOptimizer.Infrastructure.BackgroundJobs;

public sealed class TaskBackgroundJob : ITaskBackgroundJob
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskHistoryRepository _historyRepository;
    private readonly IPriorityEngine _priorityEngine;
    public TaskBackgroundJob(ITaskRepository taskRepository, ITaskHistoryRepository historyRepository, IPriorityEngine priorityEngine) { _taskRepository = taskRepository; _historyRepository = historyRepository; _priorityEngine = priorityEngine; }

    public async Task RecalculatePrioritiesAsync()
    {
        const int pageSize = 100;
        var page = 1;
        while (true)
        {
            var result = await _taskRepository.SearchAsync(new TaskQueryDto { Page = page, PageSize = pageSize }, CancellationToken.None);
            if (result.Items.Count == 0) break;
            foreach (var task in result.Items)
            {
                var oldPriority = task.Priority;
                _priorityEngine.CalculatePriority(task);
                if (oldPriority == task.Priority) continue;
                await _taskRepository.UpdateAsync(task, CancellationToken.None);
                await _historyRepository.AddTaskHistoryAsync(new TaskHistory { Id = Guid.NewGuid(), TaskId = task.Id, OldPriority = oldPriority, NewPriority = task.Priority, OldStatus = task.Status, NewStatus = task.Status, ChangedByUserId = task.CreatedByUserId, ChangeReason = "Automatic priority recalculation" }, CancellationToken.None);
            }
            if (!result.HasNextPage) break;
            page++;
        }
    }
}
