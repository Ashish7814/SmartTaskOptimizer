using SmartTaskOptimizer.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Shared.DTOs.TaskHistory
{
    public class TaskHistoryDto
    {
        public SmartTaskOptimizer.Shared.Enums.TaskStatus OldStatus { get; set; }
        public SmartTaskOptimizer.Shared.Enums.TaskStatus NewStatus { get; set; }

        public PriorityLevel OldPriority { get; set; }
        public PriorityLevel NewPriority { get; set; }

        public DateTime ChangedAt { get; set; }
        public Guid ChangedByUserId { get; set; }
    }
}
