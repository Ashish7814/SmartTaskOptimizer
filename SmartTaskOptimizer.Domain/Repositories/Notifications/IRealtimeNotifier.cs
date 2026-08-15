namespace SmartTaskOptimizer.Domain.Repositories.Notifications;

public interface IRealtimeNotifier
{
    Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default);
    Task NotifyProjectAsync(Guid projectId, string eventName, object payload, CancellationToken cancellationToken = default);
}
