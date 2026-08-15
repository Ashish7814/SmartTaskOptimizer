using SmartTaskOptimizer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Priorities
{
    public interface IPriorityStrategy
    {
        int CalculateScore(TaskItem task);
    }
}
