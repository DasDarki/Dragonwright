namespace Dragonwright.Configuration;

/// <summary>
/// Configuration settings for CORS.
/// </summary>
public sealed class CorsConfiguration
{
    public string[] AllowedOrigins { get; set; } = [];
    
    public string[] AllowedMethods { get; set; } = [];
    
    public string[] AllowedHeaders { get; set; } = [];
}