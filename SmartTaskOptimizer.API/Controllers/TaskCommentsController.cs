using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Comments;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Shared.DTOs.Comments;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/tasks/{taskId:guid}/comments")]
[ApiController]
public sealed class TaskCommentsController : ControllerBase
{
    private readonly ITaskCommentRepository _comments;
    private readonly ITaskRepository _tasks;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IActivityRepository _activities;
    public TaskCommentsController(ITaskCommentRepository comments, ITaskRepository tasks, IProjectRepository projects, ICurrentUserService currentUser, IRealtimeNotifier notifier, IActivityRepository activities) { _comments = comments; _tasks = tasks; _projects = projects; _currentUser = currentUser; _notifier = notifier; _activities = activities; }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskCommentDto>>> Get(Guid taskId, CancellationToken cancellationToken)
    {
        if (!await HasAccess(taskId, cancellationToken)) return Forbid();
        var result = await _comments.GetByTaskIdAsync(taskId, cancellationToken);
        return Ok(result.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<TaskCommentDto>> Create(Guid taskId, [FromBody] CreateTaskCommentDto dto, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (task is null) return NotFound();
        if (task.ProjectId.HasValue && !await _projects.CanAccessAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken)) return Forbid();
        if (!task.ProjectId.HasValue && task.CreatedByUserId != _currentUser.UserId) return Forbid();
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = taskId, AuthorId = _currentUser.UserId, Body = dto.Body.Trim() };
        await _comments.AddAsync(comment, cancellationToken);
        await _comments.SaveAsync(cancellationToken);
        var saved = await _comments.GetByIdAsync(comment.Id, cancellationToken) ?? comment;
        var result = ToDto(saved);
        if (task.ProjectId.HasValue) { await _activities.AddAsync(new Activity { Id = Guid.NewGuid(), ProjectId = task.ProjectId.Value, ActorId = _currentUser.UserId, TaskId = task.Id, Action = "CommentAdded" }, cancellationToken); await _notifier.NotifyProjectAsync(task.ProjectId.Value, "commentAdded", result, cancellationToken); }
        return CreatedAtAction(nameof(Get), new { taskId }, result);
    }

    [HttpPut("{commentId:guid}")]
    public async Task<ActionResult<TaskCommentDto>> Update(Guid taskId, Guid commentId, [FromBody] UpdateTaskCommentDto dto, CancellationToken cancellationToken)
    {
        if (!await HasAccess(taskId, cancellationToken)) return Forbid();
        var comment = await _comments.GetByIdAsync(commentId, cancellationToken);
        if (comment is null || comment.TaskId != taskId) return NotFound();
        if (comment.AuthorId != _currentUser.UserId) return Forbid();
        var body = dto.Body.Trim();
        if (body.Length == 0) return BadRequest("Comment body cannot be empty.");
        comment.Body = body;
        comment.UpdatedAt = DateTime.UtcNow;
        await _comments.SaveAsync(cancellationToken);
        var result = ToDto(comment);
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (task?.ProjectId is Guid projectId)
        {
            await _activities.AddAsync(new Activity { Id = Guid.NewGuid(), ProjectId = projectId, ActorId = _currentUser.UserId, TaskId = taskId, Action = "CommentUpdated" }, cancellationToken);
            await _notifier.NotifyProjectAsync(projectId, "commentUpdated", result, cancellationToken);
        }
        return Ok(result);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid commentId, CancellationToken cancellationToken)
    {
        if (!await HasAccess(taskId, cancellationToken)) return Forbid();
        var comment = await _comments.GetByIdAsync(commentId, cancellationToken);
        if (comment is null || comment.TaskId != taskId) return NotFound();
        if (comment.AuthorId != _currentUser.UserId) return Forbid();
        comment.IsDeleted = true; comment.DeletedAt = DateTime.UtcNow; comment.UpdatedAt = DateTime.UtcNow;
        await _comments.SaveAsync(cancellationToken);
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (task?.ProjectId is Guid projectId)
        {
            await _activities.AddAsync(new Activity { Id = Guid.NewGuid(), ProjectId = projectId, ActorId = _currentUser.UserId, TaskId = taskId, Action = "CommentDeleted" }, cancellationToken);
            await _notifier.NotifyProjectAsync(projectId, "commentDeleted", new { taskId, commentId }, cancellationToken);
        }
        return NoContent();
    }

    private async Task<bool> HasAccess(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        return task is not null && (task.ProjectId.HasValue ? await _projects.CanAccessAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken) : task.CreatedByUserId == _currentUser.UserId);
    }

    private static TaskCommentDto ToDto(TaskComment x) => new() { Id = x.Id, TaskId = x.TaskId, AuthorId = x.AuthorId, AuthorName = x.Author?.FullName ?? string.Empty, Body = x.Body, CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt };
}
