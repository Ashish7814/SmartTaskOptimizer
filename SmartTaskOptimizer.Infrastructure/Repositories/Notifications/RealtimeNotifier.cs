using Microsoft.AspNetCore.SignalR;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Infrastructure.Hubs;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Notifications;

public sealed class RealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public RealtimeNotifier(IHubContext<NotificationHub> hubContext) => _hubContext = hubContext;

    public Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.User(userId.ToString()).SendAsync(eventName, payload, cancellationToken);

    public Task NotifyProjectAsync(Guid projectId, string eventName, object payload, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(ProjectGroup(projectId)).SendAsync(eventName, payload, cancellationToken);

    public static string ProjectGroup(Guid projectId) => $"project:{projectId:N}";
}
