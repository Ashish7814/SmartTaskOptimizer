using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using SmartTaskOptimizer.Application.Reports.Queries;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/reports")]
[ApiController]
public sealed class ReportController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportController(IMediator mediator) => _mediator = mediator;

    [EnableRateLimiting("expensive")]
    [HttpGet("tasks")]
    public async Task<IActionResult> ExportTasks([FromQuery] string format, CancellationToken cancellationToken)
    {
        var normalized = format?.Trim().ToLowerInvariant();
        if (normalized is not ("excel" or "pdf")) return BadRequest("Format must be excel or pdf.");
        var file = await _mediator.Send(new ExportTasksQuery(normalized), cancellationToken);
        var contentType = normalized == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/pdf";
        return File(file, contentType, $"tasks.{(normalized == "excel" ? "xlsx" : "pdf")}");
    }
}
