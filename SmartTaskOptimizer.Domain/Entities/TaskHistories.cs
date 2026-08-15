using SmartTaskOptimizer.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartTaskOptimizer.Domain.Entities
{
    public class TaskHistory : BaseEntity
    {
        public Guid TaskId { get; set; }

        public Shared.Enums.TaskStatus OldStatus { get; set; }

        public Shared.Enums.TaskStatus NewStatus { get; set; }

        public PriorityLevel OldPriority { get; set; }

        public PriorityLevel NewPriority { get; set; }

        public Guid ChangedByUserId { get; set; }

        public TaskItem Task { get; set; } = null!;

        public User ChangedByUser { get; set; } = null!;

        public string? ChangeReason { get; set; }
    }
}
