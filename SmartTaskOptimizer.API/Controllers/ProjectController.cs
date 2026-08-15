using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Project.Commands;
using SmartTaskOptimizer.Application.Project.Queries;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Shared.DTOs.Project;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/projects")]
[ApiController]
public sealed class ProjectController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IActivityRepository _activities;
    private readonly INotificationRepository _notifications;
    public ProjectController(IMediator mediator, IProjectRepository projects, IUserRepository users, ICurrentUserService currentUser, IRealtimeNotifier notifier, IActivityRepository activities, INotificationRepository notifications) { _mediator = mediator; _projects = projects; _users = users; _currentUser = currentUser; _notifier = notifier; _activities = activities; _notifications = notifications; }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProjectCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken cancellationToken) => Ok(await _mediator.Send(new GetProjectsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProjectDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateProjectCommand(id, dto), cancellationToken);
        await _activities.AddAsync(new Domain.Entities.Activity { Id = Guid.NewGuid(), ProjectId = id, ActorId = _currentUser.UserId, Action = "ProjectUpdated" }, cancellationToken);
        await _notifier.NotifyProjectAsync(id, "projectUpdated", new { id, dto.Name, dto.Description }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projects.GetProjectByIdAsync(id, _currentUser.UserId, cancellationToken);
        if (project is null) return NotFound();
        if (project.OwnerId != _currentUser.UserId) return Forbid();
        await _projects.SoftDeleteAsync(id, _currentUser.UserId, cancellationToken);
        await _notifier.NotifyProjectAsync(id, "projectDeleted", new { projectId = id }, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<IReadOnlyList<ProjectMemberDto>>> Members(Guid id, CancellationToken cancellationToken)
    {
        if (!await _projects.CanAccessAsync(id, _currentUser.UserId, cancellationToken)) return Forbid();
        var members = await _projects.GetMembersAsync(id, cancellationToken);
        return Ok(members.Select(m => new ProjectMemberDto { UserId = m.UserId, FullName = m.User.FullName, Email = m.User.Email, Role = m.Role, JoinedAt = m.JoinedAt }));
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberDto dto, CancellationToken cancellationToken)
    {
        if (!await _projects.IsManagerOrOwnerAsync(id, _currentUser.UserId, cancellationToken)) return Forbid();
        if (dto.Role is not ("Member" or "Manager")) return BadRequest("Role must be Member or Manager.");
        if (await _users.GetByIdAsync(dto.UserId, cancellationToken) is null) return NotFound("User not found.");
        if (await _projects.CanAccessAsync(id, dto.UserId, cancellationToken)) return Conflict("User is already a project member.");
        await _projects.AddMemberAsync(new Domain.Entities.ProjectMember { ProjectId = id, UserId = dto.UserId, Role = dto.Role }, cancellationToken);
        await _activities.AddAsync(new Domain.Entities.Activity { Id = Guid.NewGuid(), ProjectId = id, ActorId = _currentUser.UserId, Action = "MemberAdded", NewValue = dto.UserId.ToString() }, cancellationToken);
        await _notifications.AddAsync(new Domain.Entities.Notification { Id = Guid.NewGuid(), UserId = dto.UserId, Type = Domain.Entities.NotificationType.MemberAdded, Title = "Added to project", Message = "You were added to a project.", ProjectId = id }, cancellationToken);
        await _notifier.NotifyProjectAsync(id, "memberChanged", new { projectId = id, userId = dto.UserId, role = dto.Role }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        if (!await _projects.IsManagerOrOwnerAsync(id, _currentUser.UserId, cancellationToken)) return Forbid();
        var project = await _projects.GetProjectByIdAsync(id, _currentUser.UserId, cancellationToken);
        if (project is null) return NotFound();
        if (project.OwnerId == userId) return Conflict("The project owner cannot be removed from the project.");
        if (userId == _currentUser.UserId && project.OwnerId != _currentUser.UserId) return Conflict("Use project transfer before leaving a project you manage.");
        await _projects.RemoveMemberAsync(id, userId, cancellationToken);
        await _activities.AddAsync(new Domain.Entities.Activity { Id = Guid.NewGuid(), ProjectId = id, ActorId = _currentUser.UserId, Action = "MemberRemoved", OldValue = userId.ToString() }, cancellationToken);
        await _notifier.NotifyProjectAsync(id, "memberChanged", new { projectId = id, userId, removed = true }, cancellationToken);
        return NoContent();
    }
}
