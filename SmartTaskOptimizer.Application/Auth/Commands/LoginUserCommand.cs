using MediatR;
using SmartTaskOptimizer.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Auth.Commands
{
    public record LoginUserCommand(LoginDto Dto) : IRequest<AuthResponseDto>;
}
