using SmartTaskOptimizer.Application.Priorities;
using SmartTaskOptimizer.Application.Priorities.Strategies;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Shared.Enums;
using Xunit;

namespace SmartTaskOptimizer.UnitTests.Priorities;

public sealed class PriorityEngineTests
{
    [Fact]
    public void Produces_critical_for_urgent_work()
    {
        var engine = new PriorityEngine(new IPriorityStrategy[] { new DeadlinePriorityStrategy(), new EffortPriorityStrategy(), new StatusPriorityStrategy() });
        var task = new TaskItem { Deadline = DateTime.UtcNow.AddHours(2), EstimatedDurationMinutes = 120, Status = Shared.Enums.TaskStatus.InProgress };
        engine.CalculatePriority(task);
        Assert.Equal(PriorityLevel.Critical, task.Priority);
    }
}
