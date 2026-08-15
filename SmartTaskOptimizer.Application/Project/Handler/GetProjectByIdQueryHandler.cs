using MediatR;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Project.Queries;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Shared.DTOs.Project;

namespace SmartTaskOptimizer.Application.Project.Handler;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUserService _currentUser;
    public GetProjectByIdQueryHandler(IProjectRepository repository, ICurrentUserService currentUser) { _repository = repository; _currentUser = currentUser; }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _repository.GetProjectByIdAsync(request.ProjectId, _currentUser.UserId, cancellationToken);
        return p is null ? null : new ProjectDto { Id = p.Id, Name = p.Name, Description = p.Description, OwnerId = p.OwnerId, MemberCount = p.Members.Count, TaskCount = p.Tasks.Count };
    }
}
