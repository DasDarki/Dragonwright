using System.ComponentModel.DataAnnotations;

namespace Dragonwright.Models.Auth;

/// <summary>
/// Request model for refreshing authentication tokens.
/// </summary>
public sealed class RefreshRequest
{
    /// <summary>
    /// The current access token (can be expired).
    /// </summary>
    [Required]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// The refresh token to use for obtaining new tokens.
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = null!;
}
