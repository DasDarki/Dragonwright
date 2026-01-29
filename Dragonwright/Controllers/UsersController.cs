using System.Security.Claims;
using Dragonwright.Database;
using Dragonwright.Database.Enums;
using Dragonwright.Models.Users;
using Dragonwright.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dragonwright.Controllers;

/// <summary>
/// Controller for retrieving and managing user profiles.
/// </summary>
[Authorize]
[ApiController]
[Route("users")]
public sealed class UsersController(AppDbContext dbContext, FileStorageService fileStorageService) : ControllerBase
{
    /// <summary>
    /// Retrieves a user by ID. Use <c>@me</c> to get the authenticated user's profile.
    /// Looking up other users by ID requires the <see cref="UserRole.Admin"/> role.
    /// </summary>
    /// <param name="id">The user ID or <c>@me</c> for the current user.</param>
    /// <returns>The user's profile information.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        if (id == "@me")
        {
            var user = await dbContext.Users.FindAsync(currentUserId.Value);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(UserResponse.FromEntity(user));
        }

        if (!Guid.TryParse(id, out var targetUserId))
        {
            return BadRequest(new { message = "Invalid user ID" });
        }

        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (currentUserRole != nameof(UserRole.Admin))
        {
            return Forbid();
        }

        var targetUser = await dbContext.Users.FindAsync(targetUserId);
        if (targetUser == null)
        {
            return NotFound();
        }

        return Ok(UserResponse.FromEntity(targetUser));
    }

    /// <summary>
    /// Uploads or replaces the avatar for the authenticated user.
    /// </summary>
    /// <param name="file">The avatar image file.</param>
    /// <returns>The updated user profile.</returns>
    [HttpPut("@me/avatar")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        if (file.Length == 0)
        {
            return BadRequest(new { message = "File is empty" });
        }

        if (!file.ContentType.StartsWith("image/"))
        {
            return BadRequest(new { message = "File must be an image" });
        }

        var user = await dbContext.Users.FindAsync(currentUserId.Value);
        if (user == null)
        {
            return NotFound();
        }

        var oldAvatarId = user.AvatarId;

        await using var stream = file.OpenReadStream();
        var storedFile = await fileStorageService.StoreAsync(stream, file.FileName, file.ContentType);

        user.AvatarId = storedFile.Id;
        await dbContext.SaveChangesAsync();

        if (oldAvatarId.HasValue)
        {
            await fileStorageService.DeleteAsync(oldAvatarId.Value);
        }

        return Ok(UserResponse.FromEntity(user));
    }

    /// <summary>
    /// Removes the avatar for the authenticated user.
    /// </summary>
    /// <returns>The updated user profile.</returns>
    [HttpDelete("@me/avatar")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAvatar()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var user = await dbContext.Users.FindAsync(currentUserId.Value);
        if (user == null)
        {
            return NotFound();
        }

        var oldAvatarId = user.AvatarId;

        user.AvatarId = null;
        await dbContext.SaveChangesAsync();

        if (oldAvatarId.HasValue)
        {
            await fileStorageService.DeleteAsync(oldAvatarId.Value);
        }

        return Ok(UserResponse.FromEntity(user));
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
