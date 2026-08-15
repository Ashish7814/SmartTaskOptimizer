using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Dashboard.Queries;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/dashboard")]
[ApiController]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken) => Ok(await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken));
}
