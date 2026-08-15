using MediatR;
using Microsoft.Extensions.Logging;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Project.Commands;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Project;

namespace SmartTaskOptimizer.Application.Project.Handler;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateProjectCommandHandler> _logger;
    public CreateProjectCommandHandler(IProjectRepository repository, ICurrentUserService currentUser, ILogger<CreateProjectCommandHandler> logger) { _repository = repository; _currentUser = currentUser; _logger = logger; }

    public async Task<Guid> Handle(Commands.CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new SmartTaskOptimizer.Domain.Entities.Project { Id = Guid.NewGuid(), Name = request.dto.Name.Trim(), Description = string.IsNullOrWhiteSpace(request.dto.Description) ? null : request.dto.Description.Trim(), OwnerId = _currentUser.UserId, CreatedByUserId = _currentUser.UserId };
        await _repository.CreateWithOwnerAsync(project, new ProjectMember { ProjectId = project.Id, UserId = project.OwnerId, Role = "Owner" }, cancellationToken);
        _logger.LogInformation("Project {ProjectId} created by {OwnerId}", project.Id, project.OwnerId);
        return project.Id;
    }
}
