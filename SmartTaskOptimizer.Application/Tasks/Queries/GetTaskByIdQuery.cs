using MediatR;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Queries
{
    public record GetTaskByIdQuery(Guid TaskId) : IRequest<TaskDto?>;
}
