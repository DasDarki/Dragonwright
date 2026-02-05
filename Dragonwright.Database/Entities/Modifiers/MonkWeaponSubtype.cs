using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class MonkWeaponSubtype : ModifierSubtype
{
    public Guid? WeaponId { get; set; }
    public WeaponType? WeaponType { get; set; }
    public WeaponProperty? RequiresProperty { get; set; }
    public bool AllSimpleMeleeWeapons { get; set; }
    public bool Shortswords { get; set; }
}
