using System.Security.Claims;
using Dragonwright.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dragonwright.Attributes;

/// <summary>
/// Authorization filter that requires the authenticated user to have at least the specified role.
/// Uses hierarchical role comparison where higher roles inherit lower role permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresRoleAttribute(UserRole minimumUserRole) : Attribute, IAuthorizationFilter
{
    /// <inheritdoc />
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var userRole) || userRole < minimumUserRole)
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Requires the authenticated user to have at least the <see cref="UserRole.Team"/> role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresTeamAttribute() : RequiresRoleAttribute(UserRole.Team);

/// <summary>
/// Requires the authenticated user to have the <see cref="UserRole.Admin"/> role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresAdminAttribute() : RequiresRoleAttribute(UserRole.Admin);
