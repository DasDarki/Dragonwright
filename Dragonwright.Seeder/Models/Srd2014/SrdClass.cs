namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdClass
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int HitDie { get; set; }
    public List<SrdChoice>? ProficiencyChoices { get; set; }
    public List<SrdReference> Proficiencies { get; set; } = [];
    public List<SrdReference> SavingThrows { get; set; } = [];
    public List<SrdCountedReference>? StartingEquipment { get; set; }
    public List<SrdChoice>? StartingEquipmentOptions { get; set; }
    public string? ClassLevels { get; set; }
    public SrdMulticlassing? MultiClassing { get; set; }
    public List<SrdReference>? Subclasses { get; set; }
    public SrdSpellcasting? Spellcasting { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdMulticlassing
{
    public List<SrdPrerequisite>? Prerequisites { get; set; }
    public List<SrdChoice>? ProficiencyChoices { get; set; }
    public List<SrdReference>? Proficiencies { get; set; }
}

public class SrdPrerequisite
{
    public SrdReference? AbilityScore { get; set; }
    public int MinimumScore { get; set; }
}

public class SrdSpellcasting
{
    public int Level { get; set; }
    public SrdReference? SpellcastingAbility { get; set; }
    public List<SrdSpellcastingInfo>? Info { get; set; }
}

public class SrdSpellcastingInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Desc { get; set; } = [];
}
