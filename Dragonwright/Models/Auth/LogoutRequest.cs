using System.ComponentModel.DataAnnotations;

namespace Dragonwright.Models.Auth;

/// <summary>
/// Request model for logging out from a specific device.
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    /// The refresh token to revoke.
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = null!;
}
