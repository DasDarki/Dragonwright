using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ReplaceWeaponAbilitySubtype : ModifierSubtype
{
    public AbilityScore NewAbilityScore { get; set; }
    public bool MeleeOnly { get; set; }
    public bool RangedOnly { get; set; }
    public Guid? RestrictToWeaponId { get; set; }
    public WeaponType? RestrictToWeaponType { get; set; }
    public WeaponProperty? RequiresProperty { get; set; }
    public bool UseHigherOfStrengthOrDexterity { get; set; }
}
