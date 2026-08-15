using MediatR;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Priorities;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Application.Common.Mapping;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Tasks.Commands.Create;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly ITaskRepository _repository;
    private readonly IProjectRepository _projects;
    private readonly IPriorityEngine _priorityEngine;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IActivityRepository _activities;

    public CreateTaskCommandHandler(ITaskRepository repository, IProjectRepository projects, IPriorityEngine priorityEngine, ICurrentUserService currentUser, IRealtimeNotifier notifier, IActivityRepository activities)
    {
        _repository = repository;
        _projects = projects;
        _priorityEngine = priorityEngine;
        _currentUser = currentUser; _notifier = notifier; _activities = activities;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var dto = request.dto;
        if (dto.ProjectId.HasValue && !await _projects.CanAccessAsync(dto.ProjectId.Value, _currentUser.UserId, cancellationToken))
            throw new ForbiddenException();
        if (dto.AssigneeId.HasValue && dto.ProjectId.HasValue)
        {
            var members = await _projects.GetMembersAsync(dto.ProjectId.Value, cancellationToken);
            if (!members.Any(x => x.UserId == dto.AssigneeId.Value)) throw new ConflictException("Assignee must be a member of the project.");
        }

        if (dto.DependencyIds.Count > 0)
        {
            var dependencies = await _repository.GetByIdsAsync(dto.DependencyIds.Distinct().ToArray(), _currentUser.UserId, cancellationToken);
            if (dependencies.Count != dto.DependencyIds.Distinct().Count()) throw new ForbiddenException("One or more dependency tasks are not accessible.");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Priority = (PriorityLevel)dto.Priority,
            Deadline = dto.Deadline.ToUniversalTime(),
            EstimatedDurationMinutes = dto.EstimatedDuration,
            ProjectId = dto.ProjectId,
            AssigneeId = dto.AssigneeId,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim(),
            Status = Shared.Enums.TaskStatus.Pending,
            Progress = 0,
            CreatedByUserId = _currentUser.UserId
        };
        _priorityEngine.CalculatePriority(task);
        await _repository.AddAsync(task, cancellationToken);
        if (dto.Tags.Count > 0) await _repository.ReplaceTagsAsync(task.Id, dto.Tags, cancellationToken);
        if (dto.DependencyIds.Count > 0) await _repository.ReplaceDependenciesAsync(task.Id, dto.DependencyIds, cancellationToken);
        if (task.ProjectId.HasValue) { await _activities.AddAsync(new Domain.Entities.Activity { Id = Guid.NewGuid(), ProjectId = task.ProjectId.Value, ActorId = _currentUser.UserId, TaskId = task.Id, Action = "TaskCreated" }, cancellationToken); await _notifier.NotifyProjectAsync(task.ProjectId.Value, "taskCreated", task.ToDto(), cancellationToken); }
        return task.Id;
    }
}
