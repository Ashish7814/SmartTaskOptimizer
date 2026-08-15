using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Common.Mapping;
using SmartTaskOptimizer.Application.Tasks.Commands.Create;
using SmartTaskOptimizer.Application.Tasks.Commands.Update;
using SmartTaskOptimizer.Application.Tasks.Queries;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Shared.DTOs.Common;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/tasks")]
[ApiController]
public sealed class TaskController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITaskRepository _tasks;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IActivityRepository _activities;
    public TaskController(IMediator mediator, ITaskRepository tasks, IProjectRepository projects, ICurrentUserService currentUser, IRealtimeNotifier notifier, IActivityRepository activities) { _mediator = mediator; _tasks = tasks; _projects = projects; _currentUser = currentUser; _notifier = notifier; _activities = activities; }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateTaskCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskQueryDto query, CancellationToken cancellationToken)
    {
        if (query.ProjectId.HasValue && !await _projects.CanAccessAsync(query.ProjectId.Value, _currentUser.UserId, cancellationToken)) return Forbid();
        return Ok(await _mediator.Send(new GetTasksQuery(query), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateTaskCommand(id, dto), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] Shared.Enums.TaskStatus status, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateTaskStatusCommand(id, (int)status), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null) return NotFound();
        if (task.ProjectId.HasValue && !await _projects.IsManagerOrOwnerAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken) && task.CreatedByUserId != _currentUser.UserId) return Forbid();
        if (!task.ProjectId.HasValue && task.CreatedByUserId != _currentUser.UserId) return Forbid();
        await _tasks.SoftDeleteAsync(task, cancellationToken);
        if (task.ProjectId.HasValue) { await _activities.AddAsync(new Domain.Entities.Activity { Id = Guid.NewGuid(), ProjectId = task.ProjectId.Value, ActorId = _currentUser.UserId, TaskId = task.Id, Action = "TaskDeleted" }, cancellationToken); await _notifier.NotifyProjectAsync(task.ProjectId.Value, "taskDeleted", new { taskId = task.Id }, cancellationToken); }
        return NoContent();
    }

    [EnableRateLimiting("expensive")]
    [HttpPost("optimize")]
    public async Task<ActionResult<OptimizationResult>> Optimize([FromBody] OptimizeTasksRequest request, CancellationToken cancellationToken)
    {
        if (request.TaskIds.Count == 0) return BadRequest("At least one task is required.");
        if (request.TaskIds.Count > 100) return BadRequest("A maximum of 100 tasks can be optimized at once.");
        var tasks = await _tasks.GetByIdsAsync(request.TaskIds.Distinct().ToArray(), _currentUser.UserId, cancellationToken);
        var unauthorized = tasks.Where(t => t.ProjectId.HasValue).Where(t => t.ProjectId!.Value != Guid.Empty).ToList();
        foreach (var task in unauthorized)
            if (!await _projects.CanAccessAsync(task.ProjectId!.Value, _currentUser.UserId, cancellationToken)) return Forbid();

        var ordered = tasks.Where(t => t.Status != Shared.Enums.TaskStatus.Completed && t.Status != Shared.Enums.TaskStatus.Cancelled)
            .OrderByDescending(t => t.Priority).ThenBy(t => t.Deadline).ThenByDescending(t => t.Progress).ToList();
        var cursor = DateTime.UtcNow;
        var schedule = ordered.Select((task, index) => { var start = cursor; var end = start.AddMinutes(task.EstimatedDurationMinutes); cursor = end; return new ScheduledTask { Task = task.ToDto(), StartTime = start, EndTime = end, Order = index + 1 }; }).ToArray();
        return Ok(new OptimizationResult { OptimizedSchedule = schedule, TotalDuration = schedule.Sum(x => x.Task.EstimatedDurationMinutes), Efficiency = schedule.Length == 0 ? 0 : Math.Round(schedule.Average(x => x.Task.Progress) / 100d, 2), Suggestions = new[] { "Review dependencies before starting work.", "Break large tasks into smaller deliverables." } });
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<TaskStatistics>> Statistics([FromQuery] Guid? projectId, CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await _projects.CanAccessAsync(projectId.Value, _currentUser.UserId, cancellationToken)) return Forbid();
        return Ok(await _tasks.GetStatisticsAsync(projectId, _currentUser.UserId, cancellationToken));
    }
}
