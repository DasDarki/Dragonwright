namespace Dragonwright.Seeder.Services;

/// <summary>
/// Maintains lookup dictionaries for SRD index-to-entity-ID mappings.
/// </summary>
public class IndexLookup
{
    public Dictionary<string, Guid> Languages { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Items { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Spells { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Classes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Subclasses { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Features { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Races { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Traits { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Feats { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Backgrounds { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the index key for a specific source type.
    /// </summary>
    public static string GetSourceKey(string index, SourceType source) =>
        $"{index}_{(source == SourceType.Legacy2014 ? "2014" : "2024")}";
}
