using System.Security.Claims;
using Dragonwright.Database;
using Dragonwright.Database.Entities;
using Dragonwright.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Controllers;

/// <summary>
/// Base controller for character management providing common access control functionality.
/// </summary>
public abstract class CharacterControllerBase(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// The database context for character operations.
    /// </summary>
    protected AppDbContext DbContext { get; } = dbContext;

    /// <summary>
    /// Gets the current authenticated user's ID from the claims.
    /// </summary>
    /// <returns>The user ID if authenticated, null otherwise.</returns>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    /// <summary>
    /// Gets the current authenticated user's role from the claims.
    /// </summary>
    /// <returns>The user role if available, null otherwise.</returns>
    protected UserRole? GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var role))
            return null;
        return role;
    }

    /// <summary>
    /// Checks if the current user has access to the specified character.
    /// This method is extensible to support campaign-based access in the future.
    /// </summary>
    /// <param name="character">The character to check access for.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    protected virtual async Task<bool> HasCharacterAccessAsync(Character character)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return false;

        var role = GetCurrentUserRole();
        if (role is UserRole.Admin) return true;

        if (character.UserId == userId.Value) return true;

        return await CheckCampaignAccessAsync(character.Id, userId.Value);
    }

    /// <summary>
    /// Checks if the current user has access to the character with the specified ID.
    /// </summary>
    /// <param name="characterId">The ID of the character to check access for.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    protected async Task<bool> HasCharacterAccessAsync(Guid characterId)
    {
        var character = await DbContext.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null) return false;

        return await HasCharacterAccessAsync(character);
    }

    /// <summary>
    /// Checks if the current user can modify the specified character.
    /// </summary>
    /// <param name="character">The character to check modification access for.</param>
    /// <returns>True if the user can modify the character, false otherwise.</returns>
    protected virtual async Task<bool> CanModifyCharacterAsync(Character character)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return false;

        var role = GetCurrentUserRole();
        if (role is UserRole.Admin) return true;

        if (character.UserId == userId.Value) return true;

        return await CheckCampaignModifyAccessAsync(character.Id, userId.Value);
    }

    /// <summary>
    /// Checks if the current user can modify the character with the specified ID.
    /// </summary>
    /// <param name="characterId">The ID of the character to check modification access for.</param>
    /// <returns>True if the user can modify the character, false otherwise.</returns>
    protected async Task<bool> CanModifyCharacterAsync(Guid characterId)
    {
        var character = await DbContext.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null) return false;

        return await CanModifyCharacterAsync(character);
    }

    /// <summary>
    /// Checks campaign-based access for a character.
    /// Override this method to implement campaign access rules.
    /// </summary>
    /// <param name="characterId">The character ID to check.</param>
    /// <param name="userId">The user ID requesting access.</param>
    /// <returns>True if the user has campaign-based access, false otherwise.</returns>
    protected virtual Task<bool> CheckCampaignAccessAsync(Guid characterId, Guid userId)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Checks campaign-based modification access for a character.
    /// Override this method to implement campaign modification rules.
    /// </summary>
    /// <param name="characterId">The character ID to check.</param>
    /// <param name="userId">The user ID requesting modification access.</param>
    /// <returns>True if the user has campaign-based modification access, false otherwise.</returns>
    protected virtual Task<bool> CheckCampaignModifyAccessAsync(Guid characterId, Guid userId)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Normalizes pagination parameters to valid ranges.
    /// </summary>
    /// <param name="page">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <returns>Normalized page and page size values.</returns>
    protected static (int page, int pageSize) NormalizePagination(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        return (page, pageSize);
    }
}
