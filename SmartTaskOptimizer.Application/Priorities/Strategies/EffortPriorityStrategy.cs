using SmartTaskOptimizer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Priorities.Strategies
{
    public class EffortPriorityStrategy : IPriorityStrategy
    {
        public int CalculateScore(TaskItem task)
        {
            if (task.EstimatedDurationMinutes >= 16) return 25;
            if (task.EstimatedDurationMinutes >= 8) return 15;

            return 5;
        }
    }
}
