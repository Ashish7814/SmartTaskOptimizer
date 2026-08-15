using MediatR;
using SmartTaskOptimizer.Application.Common.Exceptions;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Project.Commands;
using SmartTaskOptimizer.Domain.Repositories.Project;

namespace SmartTaskOptimizer.Application.Project.Handler;

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUserService _currentUser;
    public UpdateProjectCommandHandler(IProjectRepository repository, ICurrentUserService currentUser) { _repository = repository; _currentUser = currentUser; }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.IsManagerOrOwnerAsync(request.ProjectId, _currentUser.UserId, cancellationToken)) throw new ForbiddenException();
        var project = await _repository.GetProjectByIdAsync(request.ProjectId, _currentUser.UserId, cancellationToken) ?? throw new NotFoundException("Project was not found.");
        project.Name = request.dto.Name.Trim();
        project.Description = string.IsNullOrWhiteSpace(request.dto.Description) ? null : request.dto.Description.Trim();
        await _repository.UpdateProjectAsync(project, cancellationToken);
    }
}
