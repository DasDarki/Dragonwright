using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ReplaceDamageTypeSubtype : ModifierSubtype
{
    public DamageType? OriginalDamageType { get; set; }
    public DamageType NewDamageType { get; set; }
    public bool AllDamageTypes { get; set; }
    public bool SpellsOnly { get; set; }
    public bool WeaponsOnly { get; set; }
}
