using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class BonusSubtype : ModifierSubtype
{
    public BonusTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public Skill? Skill { get; set; }
    public MovementType? MovementType { get; set; }
    public int? SpellLevel { get; set; }
}
