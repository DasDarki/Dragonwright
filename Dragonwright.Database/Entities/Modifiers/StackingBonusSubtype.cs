using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class StackingBonusSubtype : ModifierSubtype
{
    public BonusTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public Skill? Skill { get; set; }
    public MovementType? MovementType { get; set; }
    public string StackGroup { get; set; } = string.Empty;
}
