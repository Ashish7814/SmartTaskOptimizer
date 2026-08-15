using MediatR;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Commands.Create
{
    public record CreateTaskCommand(CreateTaskDto dto) : IRequest<Guid>;
}
