using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Priorities
{
    public class PriorityEngine : IPriorityEngine
    {
        private readonly IEnumerable<IPriorityStrategy> _strategies;

        public PriorityEngine(IEnumerable<IPriorityStrategy> strategies)
        {
            _strategies = strategies;
        }

        public void CalculatePriority(TaskItem task)
        {
            if (!_strategies.Any())
            {
                task.Priority = PriorityLevel.Medium; // sane fallback, matches entity default
                return;
            }

            var totalScore = _strategies.Sum(s => s.CalculateScore(task));

            task.Priority = totalScore switch
            {
                >= 70 => PriorityLevel.Critical,
                >= 45 => PriorityLevel.High,
                >= 25 => PriorityLevel.Medium,
                _ => PriorityLevel.Low
            };
        }
    }
}
