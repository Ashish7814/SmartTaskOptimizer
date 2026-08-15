using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Domain.Entities
{
    public class TaskTag
    {
        public Guid TaskId { get; set; }

        public Guid TagId { get; set; }

        public TaskItem Task { get; set; } = null!;

        public Tag Tag { get; set; } = null!;
    }
}
