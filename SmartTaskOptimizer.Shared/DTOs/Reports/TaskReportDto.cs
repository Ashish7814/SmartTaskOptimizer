using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Shared.DTOs.Reports;

public sealed class TaskReportDto
{
    public string Title { get; init; } = string.Empty;
    public Enums.TaskStatus Status { get; init; }
    public PriorityLevel Priority { get; init; }
    public DateTime Deadline { get; init; }
    public DateTime DueDate => Deadline;
}
