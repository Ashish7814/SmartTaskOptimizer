using MediatR;
using SmartTaskOptimizer.Shared.DTOs.TaskHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.TaskHistory.Queries
{
    public record GetTaskHistoryQuery(Guid taskId) : IRequest<List<TaskHistoryDto>>;
}
