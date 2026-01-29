namespace Dragonwright.Configuration;

/// <summary>
/// Configuration settings for local file storage.
/// </summary>
public sealed class FileStorageConfiguration
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "FileStorage";

    /// <summary>
    /// The base directory path where files are stored on disk.
    /// </summary>
    public string BasePath { get; set; } = "Storage";

    /// <summary>
    /// The interval in minutes between orphan cleanup runs.
    /// </summary>
    public int CleanupIntervalMinutes { get; set; } = 60;
}
