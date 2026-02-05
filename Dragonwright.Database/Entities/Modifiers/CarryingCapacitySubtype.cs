namespace Dragonwright.Database.Entities.Modifiers;

public sealed class CarryingCapacitySubtype : ModifierSubtype
{
    public double Multiplier { get; set; } = 1;
    public bool CountAsSizeLarger { get; set; }
    public bool CountAsSizeSmaller { get; set; }
}
