using MediatR;
using SmartTaskOptimizer.Shared.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTaskOptimizer.Application.Dashboard.Queries
{
    public record GetDashboardStatsQuery() : IRequest<DashboardStatsDto>;
}
