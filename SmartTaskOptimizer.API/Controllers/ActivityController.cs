using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Shared.DTOs.Activities;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/activity")]
public sealed class ActivityController : ControllerBase
{
    private readonly IActivityRepository _activities;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    public ActivityController(IActivityRepository activities, IProjectRepository projects, ICurrentUserService currentUser) { _activities = activities; _projects = projects; _currentUser = currentUser; }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> Get(Guid projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        if (!await _projects.CanAccessAsync(projectId, _currentUser.UserId, cancellationToken)) return Forbid();
        var result = await _activities.GetByProjectAsync(projectId, page, pageSize, cancellationToken);
        return Ok(result.Select(x => new ActivityDto { Id = x.Id, ProjectId = x.ProjectId, ActorId = x.ActorId, ActorName = x.Actor?.FullName ?? string.Empty, TaskId = x.TaskId, Action = x.Action, Field = x.Field, OldValue = x.OldValue, NewValue = x.NewValue, CreatedAt = x.CreatedAt }));
    }
}
