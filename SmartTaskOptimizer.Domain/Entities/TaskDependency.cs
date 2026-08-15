using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Domain.Entities
{
    public class TaskDependency
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }

        public Guid DependsOnTaskId { get; set; }

        public TaskItem Task { get; set; } = null!;

        public TaskItem DependsOnTask { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
