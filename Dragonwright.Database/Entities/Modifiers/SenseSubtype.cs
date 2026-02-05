using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class SenseSubtype : ModifierSubtype
{
    public SenseType SenseType { get; set; }
    public int RangeInFeet { get; set; }
}
