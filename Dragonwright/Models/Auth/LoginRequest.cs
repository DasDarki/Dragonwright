using System.ComponentModel.DataAnnotations;

namespace Dragonwright.Models.Auth;

/// <summary>
/// Request model for user login.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// The username of the account.
    /// </summary>
    [Required]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password of the account.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;

    /// <summary>
    /// A unique identifier for the device/browser.
    /// </summary>
    [Required]
    public string DeviceId { get; set; } = null!;

    /// <summary>
    /// An optional human-readable name for the device (e.g. "Chrome on Windows").
    /// </summary>
    public string? DeviceName { get; set; }
}
