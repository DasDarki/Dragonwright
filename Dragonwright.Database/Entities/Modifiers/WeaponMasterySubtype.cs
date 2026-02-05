using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class WeaponMasterySubtype : ModifierSubtype
{
    public Mastery Mastery { get; set; }
    public Guid? RestrictToWeaponId { get; set; }
    public WeaponType? RestrictToWeaponType { get; set; }
    public bool AnyWeaponWithMasteryProperty { get; set; }
}
