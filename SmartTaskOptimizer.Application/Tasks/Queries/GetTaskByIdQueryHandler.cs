using MediatR;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Common.Mapping;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Shared.DTOs.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Queries;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly ITaskRepository _repository;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    public GetTaskByIdQueryHandler(ITaskRepository repository, IProjectRepository projects, ICurrentUserService currentUser) { _repository = repository; _projects = projects; _currentUser = currentUser; }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null) return null;
        if (task.ProjectId.HasValue) { if (!await _projects.CanAccessAsync(task.ProjectId.Value, _currentUser.UserId, cancellationToken)) throw new ForbiddenException(); }
        else if (task.CreatedByUserId != _currentUser.UserId) throw new ForbiddenException();
        return task.ToDto();
    }
}
