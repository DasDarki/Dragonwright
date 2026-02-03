using System.Security.Claims;
using Dragonwright.Database.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Dragonwright.Controllers;

public abstract class ContentControllerBase : ControllerBase
{
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    protected UserRole? GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var role))
            return null;
        return role;
    }

    protected bool ValidateSourcePermission(SourceType source)
    {
        if (source == SourceType.Homebrew)
            return true;
        var role = GetCurrentUserRole();
        return role is UserRole.Team or UserRole.Admin;
    }

    protected bool CanModifyContent(Guid? sourceCreatorId)
    {
        var role = GetCurrentUserRole();
        if (role is UserRole.Team or UserRole.Admin)
            return true;
        var userId = GetCurrentUserId();
        return userId != null && sourceCreatorId == userId;
    }

    protected static (int page, int pageSize) NormalizePagination(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        return (page, pageSize);
    }
}
