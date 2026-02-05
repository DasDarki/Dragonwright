using Dragonwright.Seeder.Models.Srd2014;

namespace Dragonwright.Seeder.Models.Srd2024;

public class SrdFeat2024
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public SrdFeatPrerequisites2024? Prerequisites { get; set; }
    public SrdChoice? PrerequisiteOptions { get; set; }
    public string? Repeatable { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdFeatPrerequisites2024
{
    public int? MinimumLevel { get; set; }
    public string? FeatureNamed { get; set; }
}

public class SrdScorePrerequisite
{
    public string OptionType { get; set; } = string.Empty;
    public SrdReference? AbilityScore { get; set; }
    public int MinimumScore { get; set; }
}
