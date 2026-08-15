using SmartTaskOptimizer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Priorities.Strategies
{
    public class StatusPriorityStrategy : IPriorityStrategy
    {
        public int CalculateScore(TaskItem task)
        {
            return task.Status switch
            {
                SmartTaskOptimizer.Shared.Enums.TaskStatus.OnHold => 10,
                SmartTaskOptimizer.Shared.Enums.TaskStatus.Completed => 20,
                SmartTaskOptimizer.Shared.Enums.TaskStatus.InProgress => 30,
                _ => 0
            };
        }
    }
}
