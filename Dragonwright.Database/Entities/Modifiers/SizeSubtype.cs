using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class SizeSubtype : ModifierSubtype
{
    public Size Size { get; set; }
    public bool IsIncrease { get; set; }
    public bool IsDecrease { get; set; }
    public int SizeSteps { get; set; } = 1;
}
