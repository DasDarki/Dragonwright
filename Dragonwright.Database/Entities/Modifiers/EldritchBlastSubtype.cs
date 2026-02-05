using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class EldritchBlastSubtype : ModifierSubtype
{
    public int AdditionalDamageFixed { get; set; }
    public AbilityScore? AdditionalDamageAbilityModifier { get; set; }
    public int PushDistanceInFeet { get; set; }
    public int PullDistanceInFeet { get; set; }
    public int AdditionalRangeInFeet { get; set; }
    public bool ReduceSpeedByTen { get; set; }
}
