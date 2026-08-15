using MediatR;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Common.Mapping;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Tasks.Commands.Update;

public sealed class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand>
{
    private readonly ITaskRepository _repository;
    private readonly ITaskHistoryRepository _historyRepository;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IActivityRepository _activities;
    private readonly INotificationRepository _notifications;

    public UpdateTaskStatusCommandHandler(ITaskRepository repository, ITaskHistoryRepository historyRepository, IProjectRepository projects, ICurrentUserService currentUser, IRealtimeNotifier notifier, IActivityRepository activities, INotificationRepository notifications)
    {
        _repository = repository;
        _historyRepository = historyRepository;
        _projects = projects;
        _currentUser = currentUser;
        _notifier = notifier;
        _activities = activities;
        _notifications = notifications;
    }

    public async Task Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(Shared.Enums.TaskStatus), request.Status)) throw new ConflictException("Invalid task status.");
        var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken) ?? throw new NotFoundException("Task was not found.");
        if (task.ProjectId.HasValue) { if (!await _projects.CanAccessAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken)) throw new ForbiddenException(); }
        else if (task.CreatedByUserId != _currentUser.UserId) throw new ForbiddenException();

        var oldStatus = task.Status;
        if (oldStatus == (Shared.Enums.TaskStatus)request.Status) return;
        var newStatus = (Shared.Enums.TaskStatus)request.Status;
        var allowed = (task.Status, newStatus) switch
        {
            (Shared.Enums.TaskStatus.Pending, Shared.Enums.TaskStatus.InProgress or Shared.Enums.TaskStatus.OnHold or Shared.Enums.TaskStatus.Cancelled) => true,
            (Shared.Enums.TaskStatus.InProgress, Shared.Enums.TaskStatus.Pending or Shared.Enums.TaskStatus.Completed or Shared.Enums.TaskStatus.OnHold or Shared.Enums.TaskStatus.Cancelled) => true,
            (Shared.Enums.TaskStatus.OnHold, Shared.Enums.TaskStatus.InProgress or Shared.Enums.TaskStatus.Cancelled or Shared.Enums.TaskStatus.Pending) => true,
            (Shared.Enums.TaskStatus.Completed, Shared.Enums.TaskStatus.InProgress) => true,
            (Shared.Enums.TaskStatus.Cancelled, Shared.Enums.TaskStatus.Pending) => true,
            _ => false
        };
        if (!allowed) throw new ConflictException($"Cannot change task status from {task.Status} to {newStatus}.");
        task.Status = newStatus;
        if (task.Status == Shared.Enums.TaskStatus.InProgress && task.StartedAt is null) task.StartedAt = DateTime.UtcNow;
        if (task.Status == Shared.Enums.TaskStatus.Completed)
        {
            task.Progress = 100;
            task.CompletedAt = DateTime.UtcNow;
        }
        else task.CompletedAt = null;

        await _repository.UpdateAsync(task, cancellationToken);
        await _historyRepository.AddTaskHistoryAsync(new SmartTaskOptimizer.Domain.Entities.TaskHistory
        {
            Id = Guid.NewGuid(), TaskId = task.Id, OldStatus = oldStatus, NewStatus = task.Status,
            OldPriority = task.Priority, NewPriority = task.Priority, ChangedByUserId = _currentUser.UserId,
            ChangeReason = "Status changed"
        }, cancellationToken);

        var dto = task.ToDto();
        if (task.ProjectId.HasValue)
        {
            await _activities.AddAsync(new Activity { Id = Guid.NewGuid(), ProjectId = task.ProjectId.Value, ActorId = _currentUser.UserId, TaskId = task.Id, Action = "TaskStatusChanged", Field = "Status", OldValue = oldStatus.ToString(), NewValue = task.Status.ToString() }, cancellationToken);
            await _notifier.NotifyProjectAsync(task.ProjectId.Value, "taskStatusChanged", dto, cancellationToken);
        }
        if (task.AssigneeId.HasValue)
        {
            var notification = new Domain.Entities.Notification { Id = Guid.NewGuid(), UserId = task.AssigneeId.Value, Type = Domain.Entities.NotificationType.TaskStatusChanged, Title = "Task status changed", Message = $"{task.Title} is now {task.Status}", ProjectId = task.ProjectId, TaskId = task.Id };
            await _notifications.AddAsync(notification, cancellationToken);
            await _notifier.NotifyUserAsync(task.AssigneeId.Value, "notification", new { title = notification.Title, message = notification.Message, taskId = task.Id }, cancellationToken);
        }
    }
}
