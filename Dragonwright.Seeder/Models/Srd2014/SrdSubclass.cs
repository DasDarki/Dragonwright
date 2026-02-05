namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdSubclass
{
    public string Index { get; set; } = string.Empty;
    public SrdReference? Class { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SubclassFlavor { get; set; }
    public List<string> Desc { get; set; } = [];
    public string? SubclassLevels { get; set; }
    public List<SrdSubclassSpell>? Spells { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SrdSubclassSpell
{
    public List<SrdSubclassSpellPrerequisite>? Prerequisites { get; set; }
    public SrdReference? Spell { get; set; }
}

public class SrdSubclassSpellPrerequisite
{
    public string Index { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
