namespace Dragonwright.Configuration;

/// <summary>
/// Configuration settings for JWT authentication.
/// </summary>
public sealed class AuthConfiguration
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "AuthConfiguration";

    /// <summary>
    /// The secret key used to sign JWT tokens.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// The issuer of the JWT tokens.
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// The intended audience of the JWT tokens.
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// The lifetime of access tokens in minutes.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// The lifetime of refresh tokens in minutes.
    /// </summary>
    public int RefreshTokenExpirationMinutes { get; set; } = 1440;

    /// <summary>
    /// The absolute lifetime of a refresh token family in days.
    /// </summary>
    public int RefreshTokenAbsoluteExpirationDays { get; set; } = 365;
}
