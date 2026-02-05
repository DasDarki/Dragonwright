namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdLanguage
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Desc { get; set; }
    public string Type { get; set; } = string.Empty;
    public List<string> TypicalSpeakers { get; set; } = [];
    public string? Script { get; set; }
    public string Url { get; set; } = string.Empty;
}
