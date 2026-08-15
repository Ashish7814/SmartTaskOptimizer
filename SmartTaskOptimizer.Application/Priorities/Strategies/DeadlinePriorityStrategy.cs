using SmartTaskOptimizer.Application.Priorities;
using SmartTaskOptimizer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Priorities.Strategies
{
    public class DeadlinePriorityStrategy : IPriorityStrategy
    {
        public int CalculateScore(TaskItem task)
        {
            var daysLeft = (task.Deadline.Date - DateTime.UtcNow.Date).Days;

            if (daysLeft <= 1) return 40;
            if (daysLeft <= 3) return 30;
            if (daysLeft <= 7) return 20;

            return 10;
        }
    }
}
