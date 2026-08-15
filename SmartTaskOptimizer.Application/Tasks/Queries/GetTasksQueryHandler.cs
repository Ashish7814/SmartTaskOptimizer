using MediatR;
using SmartTaskOptimizer.Application.Common.Mapping;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Shared.DTOs.Common;
using SmartTaskOptimizer.Shared.DTOs.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Queries;

public sealed class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResult<TaskDto>>
{
    private readonly ITaskRepository _repository;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    public GetTasksQueryHandler(ITaskRepository repository, IProjectRepository projects, ICurrentUserService currentUser) { _repository = repository; _projects = projects; _currentUser = currentUser; }

    public async Task<PagedResult<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        if (request.Query.ProjectId.HasValue && !await _projects.CanAccessAsync(request.Query.ProjectId.Value, _currentUser.UserId, cancellationToken))
            throw new ForbiddenException();
        var query = new TaskQueryDto
        {
            ViewerUserId = _currentUser.UserId, ProjectId = request.Query.ProjectId, Status = request.Query.Status, Priority = request.Query.Priority, AssigneeId = request.Query.AssigneeId, Search = request.Query.Search, Category = request.Query.Category, Tag = request.Query.Tag, IncludeCompleted = request.Query.IncludeCompleted, SortBy = request.Query.SortBy, Descending = request.Query.Descending, Page = request.Query.Page, PageSize = request.Query.PageSize
        };
        var result = await _repository.SearchAsync(query, cancellationToken);
        return new PagedResult<TaskDto>(result.Items.Select(x => x.ToDto()).ToArray(), result.Page, result.PageSize, result.TotalCount);
    }

}
