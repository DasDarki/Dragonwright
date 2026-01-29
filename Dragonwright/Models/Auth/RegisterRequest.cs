using System.ComponentModel.DataAnnotations;

namespace Dragonwright.Models.Auth;

/// <summary>
/// Request model for user registration.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// The desired username.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(32)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password for the account.
    /// </summary>
    [Required]
    [MinLength(8)]
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
