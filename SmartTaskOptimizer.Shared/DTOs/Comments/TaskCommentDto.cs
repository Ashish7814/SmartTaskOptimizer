using System.ComponentModel.DataAnnotations;

namespace SmartTaskOptimizer.Shared.DTOs.Comments;

public sealed class CreateTaskCommentDto
{
    [Required, StringLength(10000)]
    public string Body { get; init; } = string.Empty;
}

public sealed class UpdateTaskCommentDto
{
    [Required, StringLength(10000)]
    public string Body { get; init; } = string.Empty;
}

public sealed class TaskCommentDto
{
    public Guid Id { get; init; }
    public Guid TaskId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
