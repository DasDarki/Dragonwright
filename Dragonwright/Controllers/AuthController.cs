using System.Security.Claims;
using Dragonwright.Models.Auth;
using Dragonwright.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dragonwright.Controllers;

/// <summary>
/// Controller handling authentication operations including registration, login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <returns>Authentication tokens if successful, or a conflict status if the username exists.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (result == null)
        {
            return Conflict(new { message = "Username already exists" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>Authentication tokens if successful, or unauthorized status if credentials are invalid.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Refreshes the authentication tokens using a valid refresh token.
    /// </summary>
    /// <param name="request">The current access token and refresh token.</param>
    /// <returns>New authentication tokens if successful, or unauthorized status if refresh fails.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await authService.RefreshAsync(request);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Logs out the current device by revoking the provided refresh token.
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <returns>Success status if logout completed.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var success = await authService.LogoutAsync(userId.Value, request.RefreshToken);

        if (!success)
        {
            return BadRequest(new { message = "Invalid refresh token" });
        }

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Logs out from all devices by revoking all refresh tokens for the current user.
    /// </summary>
    /// <returns>The number of sessions terminated.</returns>
    [Authorize]
    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var count = await authService.LogoutAllAsync(userId.Value);

        return Ok(new { message = $"Logged out from {count} session(s)" });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
