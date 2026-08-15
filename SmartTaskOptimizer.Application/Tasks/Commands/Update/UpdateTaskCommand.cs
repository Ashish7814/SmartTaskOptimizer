using MediatR;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Commands.Update
{
    public record UpdateTaskCommand(Guid TaskId, UpdateTaskDto dto) : IRequest;

}
