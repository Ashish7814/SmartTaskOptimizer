using MediatR;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Common.Mapping;
using SmartTaskOptimizer.Application.Priorities;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Tasks.Commands.Update;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand>
{
    private readonly ITaskRepository _repository;
    private readonly IProjectRepository _projects;
    private readonly IPriorityEngine _priorityEngine;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IActivityRepository _activities;
    private readonly INotificationRepository _notifications;

    public UpdateTaskCommandHandler(ITaskRepository repository, IProjectRepository projects, IPriorityEngine priorityEngine, ICurrentUserService currentUser, IRealtimeNotifier notifier, IActivityRepository activities, INotificationRepository notifications)
    {
        _repository = repository;
        _projects = projects;
        _priorityEngine = priorityEngine;
        _currentUser = currentUser;
        _notifier = notifier;
        _activities = activities;
        _notifications = notifications;
    }

    public async Task Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken) ?? throw new NotFoundException("Task was not found.");
        if (task.ProjectId.HasValue) { if (!await _projects.CanAccessAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken)) throw new ForbiddenException(); }
        else if (task.CreatedByUserId != _currentUser.UserId) throw new ForbiddenException();
        if (request.dto.RowVersion is not null && task.RowVersion.Length > 0 && !task.RowVersion.SequenceEqual(request.dto.RowVersion))
            throw new ConflictException("The task was modified by another user. Refresh and try again.");

        var dto = request.dto;
        if (dto.DependencyIds is not null && dto.DependencyIds.Count > 0)
        {
            var dependencies = await _repository.GetByIdsAsync(dto.DependencyIds.Distinct().ToArray(), _currentUser.UserId, cancellationToken);
            if (dependencies.Count != dto.DependencyIds.Distinct().Count()) throw new ForbiddenException("One or more dependency tasks are not accessible.");
        }
        var oldAssigneeId = task.AssigneeId;
        if (dto.Title is not null) task.Title = dto.Title.Trim();
        if (dto.Description is not null) task.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        if (dto.Priority.HasValue) task.Priority = (PriorityLevel)dto.Priority.Value;
        if (dto.EstimatedDuration.HasValue) task.EstimatedDurationMinutes = dto.EstimatedDuration.Value;
        if (dto.Deadline.HasValue) task.Deadline = dto.Deadline.Value.ToUniversalTime();
        if (dto.Category is not null) task.Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        if (dto.AssigneeId.HasValue && task.ProjectId.HasValue)
        {
            var members = await _projects.GetMembersAsync(task.ProjectId.Value, cancellationToken);
            if (!members.Any(x => x.UserId == dto.AssigneeId.Value)) throw new ConflictException("Assignee must be a member of the project.");
        }
        if (dto.AssigneeId.HasValue) task.AssigneeId = dto.AssigneeId;
        if (dto.Progress.HasValue) task.Progress = dto.Progress.Value;
        if (dto.Status.HasValue) ApplyStatus(task, (Shared.Enums.TaskStatus)dto.Status.Value);

        _priorityEngine.CalculatePriority(task);
        await _repository.UpdateAsync(task, cancellationToken);
        if (dto.Tags is not null) await _repository.ReplaceTagsAsync(task.Id, dto.Tags, cancellationToken);
        if (dto.DependencyIds is not null) await _repository.ReplaceDependenciesAsync(task.Id, dto.DependencyIds, cancellationToken);

        if (task.AssigneeId.HasValue && task.AssigneeId != oldAssigneeId)
        {
            var notification = new Domain.Entities.Notification { Id = Guid.NewGuid(), UserId = task.AssigneeId.Value, Type = Domain.Entities.NotificationType.TaskAssigned, Title = "Task assigned", Message = $"You are assigned to {task.Title}.", ProjectId = task.ProjectId, TaskId = task.Id };
            await _notifications.AddAsync(notification, cancellationToken);
            await _notifier.NotifyUserAsync(task.AssigneeId.Value, "notification", new { title = notification.Title, message = notification.Message, taskId = task.Id }, cancellationToken);
        }
        if (task.ProjectId.HasValue)
        {
            await _activities.AddAsync(new Domain.Entities.Activity { Id = Guid.NewGuid(), ProjectId = task.ProjectId.Value, ActorId = _currentUser.UserId, TaskId = task.Id, Action = "TaskUpdated" }, cancellationToken);
            await _notifier.NotifyProjectAsync(task.ProjectId.Value, "taskUpdated", task.ToDto(), cancellationToken);
        }
    }

    private static void ApplyStatus(Domain.Entities.TaskItem task, Shared.Enums.TaskStatus status)
    {
        if (task.Status == status) return;
        var allowed = (task.Status, status) switch
        {
            (Shared.Enums.TaskStatus.Pending, Shared.Enums.TaskStatus.InProgress or Shared.Enums.TaskStatus.OnHold or Shared.Enums.TaskStatus.Cancelled) => true,
            (Shared.Enums.TaskStatus.InProgress, Shared.Enums.TaskStatus.Pending or Shared.Enums.TaskStatus.Completed or Shared.Enums.TaskStatus.OnHold or Shared.Enums.TaskStatus.Cancelled) => true,
            (Shared.Enums.TaskStatus.OnHold, Shared.Enums.TaskStatus.InProgress or Shared.Enums.TaskStatus.Cancelled or Shared.Enums.TaskStatus.Pending) => true,
            (Shared.Enums.TaskStatus.Completed, Shared.Enums.TaskStatus.InProgress) => true,
            (Shared.Enums.TaskStatus.Cancelled, Shared.Enums.TaskStatus.Pending) => true,
            _ => false
        };
        if (!allowed) throw new ConflictException($"Cannot change task status from {task.Status} to {status}.");
        task.Status = status;
        if (status == Shared.Enums.TaskStatus.InProgress && task.StartedAt is null) task.StartedAt = DateTime.UtcNow;
        if (status == Shared.Enums.TaskStatus.Completed)
        {
            task.Progress = 100;
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (task.CompletedAt is not null) task.CompletedAt = null;
    }
}
