using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Shared.DTOs.Users;

namespace SmartTaskOptimizer.API.Controllers;

[Authorize]
[Route("api/profile")]
[ApiController]
public sealed class ProfileController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;
    private readonly PasswordHasher<User> _hasher = new();
    public ProfileController(IUserRepository users, ICurrentUserService currentUser) { _users = users; _currentUser = currentUser; }

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> Get(CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);
        return user is null ? NotFound() : Ok(ToDto(user));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
    {
        await _users.UpdateProfileAsync(_currentUser.UserId, dto.FullName.Trim(), cancellationToken);
        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        if (dto.NewPassword.Length < 8 || !dto.NewPassword.Any(char.IsUpper) || !dto.NewPassword.Any(char.IsLower) || !dto.NewPassword.Any(char.IsDigit)) return BadRequest("New password must be at least 8 characters and contain upper, lower, and numeric characters.");
        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user is null) return NotFound();
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (result == PasswordVerificationResult.Failed) return BadRequest("Current password is incorrect.");
        var hash = _hasher.HashPassword(user, dto.NewPassword);
        await _users.UpdatePasswordAsync(user.Id, hash, cancellationToken);
        return NoContent();
    }

    private static UserProfileDto ToDto(User user) => new() { Id = user.Id, FullName = user.FullName, Email = user.Email, Role = user.Role.ToString() };
}
