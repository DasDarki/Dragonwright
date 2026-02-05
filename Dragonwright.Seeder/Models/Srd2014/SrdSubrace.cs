namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdSubrace
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SrdReference? Race { get; set; }
    public string Desc { get; set; } = string.Empty;
    public List<SrdAbilityBonus> AbilityBonuses { get; set; } = [];
    public List<SrdReference>? StartingProficiencies { get; set; }
    public List<SrdReference>? Languages { get; set; }
    public SrdChoice? LanguageOptions { get; set; }
    public List<SrdReference> RacialTraits { get; set; } = [];
    public string Url { get; set; } = string.Empty;
}
