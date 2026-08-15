using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Infrastructure.Data;
using SmartTaskOptimizer.Shared.DTOs.Common;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Tasks;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    public TaskRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<TaskItem>> GetByIdsAsync(IReadOnlyList<Guid> ids, Guid viewerUserId, CancellationToken cancellationToken) =>
        _context.Tasks.Include(t => t.CreatedByUser).Include(t => t.Assignee).Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Dependencies)
            .Where(t => ids.Contains(t.Id) && (!t.ProjectId.HasValue ? t.CreatedByUserId == viewerUserId : t.Project!.OwnerId == viewerUserId || t.Project.Members.Any(m => m.UserId == viewerUserId) || _context.Users.Any(u => u.Id == viewerUserId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin))).ToListAsync(cancellationToken);

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Tasks
            .Include(t => t.CreatedByUser)
            .Include(t => t.Assignee)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Dependencies)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<PagedResult<TaskItem>> SearchAsync(TaskQueryDto query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        IQueryable<TaskItem> q = _context.Tasks.AsNoTracking()
            .Include(t => t.CreatedByUser)
            .Include(t => t.Assignee)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Dependencies);

        if (query.ViewerUserId.HasValue)
        {
            q = q.Where(t =>
                t.ProjectId.HasValue
                    ? t.Project!.OwnerId == query.ViewerUserId.Value || t.Project.Members.Any(m => m.UserId == query.ViewerUserId.Value) || _context.Users.Any(u => u.Id == query.ViewerUserId.Value && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin)
                    : t.CreatedByUserId == query.ViewerUserId.Value);
        }
        if (query.ProjectId.HasValue) q = q.Where(t => t.ProjectId == query.ProjectId);
        if (query.Status.HasValue) q = q.Where(t => t.Status == query.Status);
        if (query.Priority.HasValue) q = q.Where(t => t.Priority == query.Priority);
        if (query.AssigneeId.HasValue) q = q.Where(t => t.AssigneeId == query.AssigneeId);
        if (!query.IncludeCompleted) q = q.Where(t => t.Status != Shared.Enums.TaskStatus.Completed && t.Status != Shared.Enums.TaskStatus.Cancelled);
        if (!string.IsNullOrWhiteSpace(query.Category)) q = q.Where(t => t.Category == query.Category);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            q = q.Where(t => EF.Functions.Like(t.Title, $"%{search}%") || (t.Description != null && EF.Functions.Like(t.Description, $"%{search}%")));
        }
        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim();
            q = q.Where(t => t.TaskTags.Any(tt => tt.Tag.Name == tag));
        }

        q = query.SortBy.Trim().ToLowerInvariant() switch
        {
            "title" => query.Descending ? q.OrderByDescending(t => t.Title) : q.OrderBy(t => t.Title),
            "deadline" => query.Descending ? q.OrderByDescending(t => t.Deadline) : q.OrderBy(t => t.Deadline),
            "priority" => query.Descending ? q.OrderByDescending(t => t.Priority) : q.OrderBy(t => t.Priority),
            "status" => query.Descending ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status),
            _ => query.Descending ? q.OrderByDescending(t => t.UpdatedAt) : q.OrderBy(t => t.UpdatedAt)
        };

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<TaskItem>(items, page, pageSize, total);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(TaskItem task, CancellationToken cancellationToken)
    {
        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskStatistics> GetStatisticsAsync(Guid? projectId, Guid viewerUserId, CancellationToken cancellationToken)
    {
        var q = _context.Tasks.AsNoTracking().Where(t => t.ProjectId.HasValue ? t.Project!.OwnerId == viewerUserId || t.Project.Members.Any(m => m.UserId == viewerUserId) || _context.Users.Any(u => u.Id == viewerUserId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) : t.CreatedByUserId == viewerUserId);
        if (projectId.HasValue) q = q.Where(t => t.ProjectId == projectId);
        var total = await q.CountAsync(cancellationToken);
        var completed = await q.CountAsync(t => t.Status == Shared.Enums.TaskStatus.Completed, cancellationToken);
        var inProgress = await q.CountAsync(t => t.Status == Shared.Enums.TaskStatus.InProgress, cancellationToken);
        var pending = await q.CountAsync(t => t.Status == Shared.Enums.TaskStatus.Pending, cancellationToken);
        var overdue = await q.CountAsync(t => t.Deadline < DateTime.UtcNow && t.Status != Shared.Enums.TaskStatus.Completed && t.Status != Shared.Enums.TaskStatus.Cancelled, cancellationToken);
        var avg = total == 0 ? 0 : (int)await q.AverageAsync(t => (double)t.EstimatedDurationMinutes, cancellationToken);
        var byPriority = await q.GroupBy(t => t.Priority).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var byStatus = await q.GroupBy(t => t.Status).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        return new TaskStatistics { Total = total, Completed = completed, InProgress = inProgress, Pending = pending, Overdue = overdue, CompletionRate = total == 0 ? 0 : Math.Round(completed * 100d / total, 2), AverageDurationMinutes = avg, ByPriority = byPriority, ByStatus = byStatus };
    }

    public async Task ReplaceDependenciesAsync(Guid taskId, IReadOnlyList<Guid> dependencyIds, CancellationToken cancellationToken)
    {
        var ids = dependencyIds.Distinct().Where(x => x != taskId).Take(50).ToArray();
        if (ids.Length > 0)
        {
            var valid = await _context.Tasks.Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
            if (valid.Count != ids.Length) throw new InvalidOperationException("One or more dependency tasks do not exist.");
        }
        var existing = await _context.TaskDependencies.Where(x => x.TaskId == taskId).ToListAsync(cancellationToken);
        _context.TaskDependencies.RemoveRange(existing);
        foreach (var id in ids) _context.TaskDependencies.Add(new TaskDependency { Id = Guid.NewGuid(), TaskId = taskId, DependsOnTaskId = id });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceTagsAsync(Guid taskId, IReadOnlyList<string> tags, CancellationToken cancellationToken)
    {
        var normalized = tags.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Take(20).ToArray();
        var existing = await _context.TaskTags.Where(x => x.TaskId == taskId).ToListAsync(cancellationToken);
        _context.TaskTags.RemoveRange(existing);

        foreach (var name in normalized)
        {
            var tag = await _context.Tags.SingleOrDefaultAsync(x => x.Name == name, cancellationToken);
            if (tag is null)
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = name };
                _context.Tags.Add(tag);
            }
            _context.TaskTags.Add(new TaskTag { TaskId = taskId, TagId = tag.Id });
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
