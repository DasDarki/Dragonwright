using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class SetSubtype : ModifierSubtype
{
    public BonusTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public MovementType? MovementType { get; set; }
    public int Value { get; set; }
}
