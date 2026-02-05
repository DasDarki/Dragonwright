namespace Dragonwright.Database.Entities.Modifiers;

public sealed class FeatSubtype : ModifierSubtype
{
    public Guid? FeatId { get; set; }
    public bool AnyFeat { get; set; }
    public int? MaxFeatLevel { get; set; }
}
