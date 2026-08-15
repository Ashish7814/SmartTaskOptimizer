using Microsoft.AspNetCore.Http;
using SmartTaskOptimizer.Application.Common.Interfaces;
using System.Security.Claims;

namespace SmartTaskOptimizer.Application.Service.Implentation;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
            if (!Guid.TryParse(value, out var id)) throw new UnauthorizedAccessException("Authentication is required.");
            return id;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
}
