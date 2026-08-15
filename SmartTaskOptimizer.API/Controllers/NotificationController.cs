using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Shared.DTOs.Notifications;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/notifications")]
[ApiController]
public sealed class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserService _currentUser;
    public NotificationController(INotificationRepository notifications, ICurrentUserService currentUser) { _notifications = notifications; _currentUser = currentUser; }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _notifications.GetForUserAsync(_currentUser.UserId, unreadOnly, page, pageSize, cancellationToken);
        return Ok(result.Select(x => new NotificationDto { Id = x.Id, Title = x.Title, Message = x.Message, Type = (int)x.Type, ProjectId = x.ProjectId, TaskId = x.TaskId, IsRead = x.IsRead, CreatedAt = x.CreatedAt }));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken) { await _notifications.MarkReadAsync(id, _currentUser.UserId, cancellationToken); return NoContent(); }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken) { await _notifications.MarkAllReadAsync(_currentUser.UserId, cancellationToken); return NoContent(); }
}
