namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdFeature
{
    public string Index { get; set; } = string.Empty;
    public SrdReference? Class { get; set; }
    public SrdReference? Subclass { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Desc { get; set; } = [];
}

public class SrdFeaturePrerequisite
{
    public string? Type { get; set; }
    public SrdReference? Feature { get; set; }
    public string? Spell { get; set; }
    public int? Level { get; set; }
}
