using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;
using SmartTaskOptimizer.Shared.DTOs.TaskHistory;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/tasks/{taskId:guid}/history")]
[ApiController]
public sealed class TaskHistoryController : ControllerBase
{
    private readonly ITaskHistoryRepository _history;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    private readonly SmartTaskOptimizer.Domain.Repositories.Tasks.ITaskRepository _tasks;
    public TaskHistoryController(ITaskHistoryRepository history, IProjectRepository projects, ICurrentUserService currentUser, SmartTaskOptimizer.Domain.Repositories.Tasks.ITaskRepository tasks) { _history = history; _projects = projects; _currentUser = currentUser; _tasks = tasks; }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskHistoryDto>>> Get(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (task is null) return NotFound();
        if (task.ProjectId.HasValue && !await _projects.CanAccessAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken)) return Forbid();
        if (!task.ProjectId.HasValue && task.CreatedByUserId != _currentUser.UserId) return Forbid();
        var result = await _history.GetByTaskIdAsync(taskId, cancellationToken);
        return Ok(result.Select(h => new TaskHistoryDto { OldStatus = h.OldStatus, NewStatus = h.NewStatus, OldPriority = h.OldPriority, NewPriority = h.NewPriority, ChangedAt = h.CreatedAt, ChangedByUserId = h.ChangedByUserId }));
    }
}
