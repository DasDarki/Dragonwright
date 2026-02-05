namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdBackground
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SrdReference> StartingProficiencies { get; set; } = [];
    public SrdChoice? LanguageOptions { get; set; }
    public List<SrdCountedReference>? StartingEquipment { get; set; }
    public List<SrdChoice>? StartingEquipmentOptions { get; set; }
    public SrdBackgroundFeature? Feature { get; set; }
    public SrdChoice? PersonalityTraits { get; set; }
    public SrdChoice? Ideals { get; set; }
    public SrdChoice? Bonds { get; set; }
    public SrdChoice? Flaws { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdBackgroundFeature
{
    public string Name { get; set; } = string.Empty;
    public List<string> Desc { get; set; } = [];
}
