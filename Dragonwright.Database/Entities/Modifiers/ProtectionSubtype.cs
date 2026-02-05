using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ProtectionSubtype : ModifierSubtype
{
    public List<DamageType> DamageTypes { get; set; } = [];
    public bool AllMagicalDamage { get; set; }
    public bool AllNonMagicalDamage { get; set; }
    public int DamageReductionFixed { get; set; }
    public int DamageReductionPerCharacterLevel { get; set; }
    public AbilityScore? DamageReductionAbilityModifier { get; set; }
}
