using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class SpeedIncreaseSubtype : ModifierSubtype
{
    public MovementType? MovementType { get; set; }
    public bool AllMovementTypes { get; set; }
    public int IncreaseInFeet { get; set; }
    public bool EqualToWalkingSpeed { get; set; }
}
