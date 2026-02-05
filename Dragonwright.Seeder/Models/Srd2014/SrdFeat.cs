namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdFeat
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SrdPrerequisite>? Prerequisites { get; set; }
    public List<string> Desc { get; set; } = [];
    public string Url { get; set; } = string.Empty;
}
