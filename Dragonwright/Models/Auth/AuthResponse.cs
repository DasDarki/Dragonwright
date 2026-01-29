namespace Dragonwright.Models.Auth;

/// <summary>
/// Response model containing authentication tokens.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// The JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// The expiration time of the access token in UTC.
    /// </summary>
    public DateTime AccessTokenExpiration { get; set; }
}
