using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Notifications;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;
    public NotificationRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Notification>> GetForUserAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _context.Notifications.AsNoTracking().Where(x => x.UserId == userId);
        if (unreadOnly) query = query.Where(x => !x.IsRead);
        return query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.SingleOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);
        if (notification is null) return;
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _context.Notifications.Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true).SetProperty(x => x.ReadAt, DateTime.UtcNow), cancellationToken);
    }
}
