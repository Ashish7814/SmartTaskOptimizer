using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Shared.DTOs.Auth;

namespace SmartTaskOptimizer.API.Controllers;

[Route("api/auth")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new RegisterUserCommand(dto), cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new LoginUserCommand(dto), cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me() => Ok(new { UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, Name = User.Identity?.Name, Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value, Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value });
}
