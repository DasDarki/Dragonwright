using Dragonwright.Seeder.Models.Srd2014;

namespace Dragonwright.Seeder.Models.Srd2024;

public class SrdBackground2024
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SrdReference> AbilityScores { get; set; } = [];
    public SrdFeatReference? Feat { get; set; }
    public List<SrdReference> Proficiencies { get; set; } = [];
    public object? EquipmentOptions { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdFeatReference
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Url { get; set; } = string.Empty;
}
