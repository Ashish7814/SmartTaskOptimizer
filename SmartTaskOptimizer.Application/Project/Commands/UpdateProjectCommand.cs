using MediatR;
using SmartTaskOptimizer.Shared.DTOs.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Project.Commands
{
    public record UpdateProjectCommand(Guid ProjectId, ProjectDto dto) : IRequest;
}
