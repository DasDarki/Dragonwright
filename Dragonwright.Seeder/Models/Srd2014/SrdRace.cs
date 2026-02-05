namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdRace
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Speed { get; set; }
    public List<SrdAbilityBonus> AbilityBonuses { get; set; } = [];
    public SrdChoice? AbilityBonusOptions { get; set; }
    public string? Alignment { get; set; }
    public string? Age { get; set; }
    public string Size { get; set; } = string.Empty;
    public string? SizeDescription { get; set; }
    public List<SrdReference> Languages { get; set; } = [];
    public SrdChoice? LanguageOptions { get; set; }
    public string? LanguageDesc { get; set; }
    public List<SrdReference> Traits { get; set; } = [];
    public List<SrdReference>? StartingProficiencies { get; set; }
    public SrdChoice? StartingProficiencyOptions { get; set; }
    public List<SrdReference>? Subraces { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdAbilityBonus
{
    public SrdReference? AbilityScore { get; set; }
    public int Bonus { get; set; }
}
