using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Shared.DTOs.Auth;

namespace SmartTaskOptimizer.API.Controllers;

[Route("api/auth")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName =
        "smarttask.refresh";

    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(
        IMediator mediator,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(
                new RegisterUserCommand(dto),
                cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress =
            HttpContext.Connection.RemoteIpAddress?
                .ToString();

        var result =
            await _mediator.Send(
                new LoginUserCommand(
                    dto,
                    ipAddress),
                cancellationToken);

        SetRefreshCookie(result.RefreshToken);

        return Ok(ToResponse(result));
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        CancellationToken cancellationToken)
    {
        var refreshToken =
            Request.Cookies[RefreshCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new
            {
                message = "Refresh token is missing."
            });
        }

        var ipAddress =
            HttpContext.Connection.RemoteIpAddress?
                .ToString();

        var result =
            await _mediator.Send(
                new RefreshTokenCommand(
                    refreshToken,
                    ipAddress),
                cancellationToken);

        SetRefreshCookie(result.RefreshToken);

        return Ok(ToResponse(result));
    }

   [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        var refreshToken =
            Request.Cookies[RefreshCookieName];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var ipAddress =
                HttpContext.Connection
                    .RemoteIpAddress?
                    .ToString();

            await _mediator.Send(
                new LogoutCommand(
                    refreshToken,
                    ipAddress),
                cancellationToken);
        }

        DeleteRefreshCookie();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            UserId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?
                .Value,

            Name = User.Identity?.Name,

            Email = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email)?
                .Value,

            Role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?
                .Value
        });
    }

    private AuthResponseDto ToResponse(
        AuthTokenResult result)
    {
        return new AuthResponseDto
        {
            Token = result.AccessToken,
            UserId = result.UserId,
            FullName = result.FullName,
            Email = result.Email,
            Role = result.Role,
            ExpiresAtUtc = result.ExpiresAtUtc
        };
    }

    private void SetRefreshCookie(
        string refreshToken)
    {
        var days =
            _configuration.GetValue(
                "Jwt:RefreshTokenDays",
                7);

        Response.Cookies.Append(
            RefreshCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = GetSameSiteMode(),
                Expires =
                    DateTimeOffset.UtcNow.AddDays(days),

                MaxAge =
                    TimeSpan.FromDays(days),

                Path = "/api/auth"
            });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(
            RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = GetSameSiteMode(),
                Path = "/api/auth"
            });
    }

    private SameSiteMode GetSameSiteMode()
    {
        var sameSite =
            _configuration["Jwt:RefreshCookieSameSite"];

        return Enum.TryParse<SameSiteMode>(
            sameSite,
            ignoreCase: true,
            out var result)
            ? result
            : SameSiteMode.None;
    }
}
