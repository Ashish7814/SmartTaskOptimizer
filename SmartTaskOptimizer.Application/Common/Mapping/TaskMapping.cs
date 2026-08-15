using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Shared.DTOs.Tasks;

namespace SmartTaskOptimizer.Application.Common.Mapping;

public static class TaskMapping
{
    public static TaskDto ToDto(this TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Priority = task.Priority,
        Status = task.Status,
        EstimatedDurationMinutes = task.EstimatedDurationMinutes,
        Deadline = task.Deadline,
        ProjectId = task.ProjectId,
        AssigneeId = task.AssigneeId,
        AssigneeName = task.Assignee?.FullName,
        CreatedByUserId = task.CreatedByUserId,
        CreatedByName = task.CreatedByUser?.FullName,
        Category = task.Category,
        Progress = task.Progress,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
        StartedAt = task.StartedAt,
        CompletedAt = task.CompletedAt,
        Tags = task.TaskTags.Where(x => x.Tag != null).Select(x => x.Tag.Name).OrderBy(x => x).ToArray(),
        DependencyIds = task.Dependencies.Select(x => x.DependsOnTaskId).ToArray(),
        RowVersion = task.RowVersion.Length == 0 ? null : task.RowVersion
    };
}
