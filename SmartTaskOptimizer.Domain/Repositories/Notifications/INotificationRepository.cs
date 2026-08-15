using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Notifications;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<List<Notification>> GetForUserAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken);
    Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken);
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken);
}
