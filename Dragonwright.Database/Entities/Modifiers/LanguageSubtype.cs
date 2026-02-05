namespace Dragonwright.Database.Entities.Modifiers;

public sealed class LanguageSubtype : ModifierSubtype
{
    public Guid? LanguageId { get; set; }
    public bool AnyLanguage { get; set; }
    public int LanguageCount { get; set; } = 1;
}
