using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class DamageSubtype : ModifierSubtype
{
    public DamageType DamageType { get; set; }
    public bool OnCriticalOnly { get; set; }
    public bool MeleeOnly { get; set; }
    public bool RangedOnly { get; set; }
    public bool SpellOnly { get; set; }
    public bool OncePerTurn { get; set; }
    public Guid? RestrictToWeaponId { get; set; }
}
