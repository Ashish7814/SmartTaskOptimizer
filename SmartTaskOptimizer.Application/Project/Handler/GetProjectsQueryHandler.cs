using MediatR;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Project.Queries;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Shared.DTOs.Project;

namespace SmartTaskOptimizer.Application.Project.Handler;

public sealed class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUserService _currentUser;
    public GetProjectsQueryHandler(IProjectRepository repository, ICurrentUserService currentUser) { _repository = repository; _currentUser = currentUser; }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _repository.GetAllProjectAsync(_currentUser.UserId, cancellationToken);
        return projects.Select(ToDto).ToList();
    }

    private static ProjectDto ToDto(Domain.Entities.Project p) => new() { Id = p.Id, Name = p.Name, Description = p.Description, OwnerId = p.OwnerId, MemberCount = p.Members.Count, TaskCount = p.Tasks.Count };
}
