using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class RangedWeaponAttackSubtype : ModifierSubtype
{
    public int AttackBonus { get; set; }
    public int DamageBonus { get; set; }
    public DamageType? AdditionalDamageType { get; set; }
    public int AdditionalDamageDiceCount { get; set; }
    public int AdditionalDamageDiceValue { get; set; }
    public int AdditionalDamageFixed { get; set; }
    public WeaponProperty? RequiresWeaponProperty { get; set; }
    public Guid? RestrictToWeaponId { get; set; }
    public bool IgnoreLongRangeDisadvantage { get; set; }
}
