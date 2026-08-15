namespace SmartTaskOptimizer.Shared.DTOs.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int Type { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? TaskId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}
