using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class NaturalWeaponSubtype : ModifierSubtype
{
    public string WeaponName { get; set; } = string.Empty;
    public DamageType DamageType { get; set; }
    public int DamageDiceCount { get; set; } = 1;
    public int DamageDiceValue { get; set; } = 4;
    public AbilityScore? AbilityScoreOverride { get; set; }
    public bool UseStrengthOrDexterity { get; set; }
    public int ReachInFeet { get; set; } = 5;
}
