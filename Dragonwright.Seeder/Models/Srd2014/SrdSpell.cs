namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdSpell
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Desc { get; set; } = [];
    public List<string>? HigherLevel { get; set; }
    public string Range { get; set; } = string.Empty;
    public List<string> Components { get; set; } = [];
    public string? Material { get; set; }
    public bool Ritual { get; set; }
    public string Duration { get; set; } = string.Empty;
    public bool Concentration { get; set; }
    public string CastingTime { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? AttackType { get; set; }
    public SrdSpellDamage? Damage { get; set; }
    public SrdSpellDc? Dc { get; set; }
    public SrdReference? School { get; set; }
    public List<SrdReference> Classes { get; set; } = [];
    public List<SrdReference>? Subclasses { get; set; }
    public SrdAreaOfEffect? AreaOfEffect { get; set; }
    public Dictionary<string, string>? HealAtSlotLevel { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdSpellDamage
{
    public SrdReference? DamageType { get; set; }
    public Dictionary<string, string>? DamageAtSlotLevel { get; set; }
    public Dictionary<string, string>? DamageAtCharacterLevel { get; set; }
}

public class SrdSpellDc
{
    public SrdReference? DcType { get; set; }
    public string? DcSuccess { get; set; }
}

public class SrdAreaOfEffect
{
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
}
