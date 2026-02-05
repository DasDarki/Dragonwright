using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class FavoredEnemySubtype : ModifierSubtype
{
    public List<CreatureType> CreatureTypes { get; set; } = [];
    public bool AdvantageOnSurvivalToTrack { get; set; } = true;
    public bool AdvantageOnIntelligenceToRecall { get; set; } = true;
    public Guid? LanguageId { get; set; }
}
