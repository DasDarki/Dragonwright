namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdTrait
{
    public string Index { get; set; } = string.Empty;
    public List<SrdReference> Races { get; set; } = [];
    public List<SrdReference>? Subraces { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Desc { get; set; } = [];
    public List<SrdReference> Proficiencies { get; set; } = [];
    public SrdChoice? ProficiencyChoices { get; set; }
    public SrdChoice? LanguageOptions { get; set; }
    public SrdTraitSpecific? TraitSpecificData { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdTraitSpecific
{
    public SrdChoice? SpellOptions { get; set; }
    public SrdChoice? SubtraitOptions { get; set; }
    public List<SrdBreathWeapon>? BreathWeapon { get; set; }
    public SrdDamageType? DamageType { get; set; }
}

public class SrdBreathWeapon
{
    public string Name { get; set; } = string.Empty;
    public List<string> Desc { get; set; } = [];
    public SrdAreaOfEffect? AreaOfEffect { get; set; }
    public SrdSpellDc? Dc { get; set; }
    public List<SrdUsage>? Usage { get; set; }
    public List<SrdBreathDamage>? Damage { get; set; }
}

public class SrdUsage
{
    public string Type { get; set; } = string.Empty;
    public int? Times { get; set; }
}

public class SrdBreathDamage
{
    public SrdReference? DamageType { get; set; }
    public Dictionary<string, string>? DamageAtCharacterLevel { get; set; }
}

public class SrdDamageType
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
