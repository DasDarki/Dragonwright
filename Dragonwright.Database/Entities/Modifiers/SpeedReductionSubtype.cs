using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class SpeedReductionSubtype : ModifierSubtype
{
    public MovementType? MovementType { get; set; }
    public bool AllMovementTypes { get; set; }
    public int ReductionInFeet { get; set; }
    public bool SetToZero { get; set; }
}
