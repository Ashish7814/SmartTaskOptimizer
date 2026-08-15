using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Infrastructure.Repositories.Notifications;
using System.Security.Claims;

namespace SmartTaskOptimizer.Infrastructure.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    private readonly IProjectRepository _projects;
    public NotificationHub(IProjectRepository projects) => _projects = projects;

    public async Task JoinProject(Guid projectId)
    {
        var userId = GetUserId();
        if (!await _projects.CanAccessAsync(projectId, userId, Context.ConnectionAborted))
            throw new HubException("You do not have access to this project.");
        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeNotifier.ProjectGroup(projectId), Context.ConnectionAborted);
    }

    public async Task LeaveProject(Guid projectId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeNotifier.ProjectGroup(projectId), Context.ConnectionAborted);

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var id)) throw new HubException("Authentication is required.");
        return id;
    }
}
