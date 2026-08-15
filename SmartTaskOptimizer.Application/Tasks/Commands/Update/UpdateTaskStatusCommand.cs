using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Commands.Update
{
    public record UpdateTaskStatusCommand(Guid TaskId, int Status) : IRequest;
}
