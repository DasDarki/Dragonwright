using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class WeaponPropertySubtype : ModifierSubtype
{
    public WeaponProperty Property { get; set; }
    public Guid? RestrictToWeaponId { get; set; }
    public WeaponType? RestrictToWeaponType { get; set; }
}
