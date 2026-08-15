namespace SmartTaskOptimizer.Shared.DTOs.Dashboard;

public sealed class DashboardStatsDto
{
    public int TotalTasks { get; init; }
    public int PendingTasks { get; init; }
    public int InProgressTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int CancelledTasks { get; init; }
    public int OnHoldTasks { get; init; }
    public int HighPriorityTasks { get; init; }
    public int CriticalPriorityTasks { get; init; }
    public int OverdueTasks { get; init; }

    // Backward-compatible aliases for the original API contract.
    public int TotaltblTasks => TotalTasks;
    public int TodotblTasks => PendingTasks;
    public int InProgresstblTasks => InProgressTasks;
    public int QAtblTasks => OnHoldTasks;
    public int DonetblTasks => CompletedTasks;
    public int HighPrioritytblTasks => HighPriorityTasks;
    public int CriticalPrioritytblTasks => CriticalPriorityTasks;
}
