using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class SetBaseSubtype : ModifierSubtype
{
    public BonusTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public MovementType? MovementType { get; set; }
    public int BaseValue { get; set; }
    public AbilityScore? AddAbilityModifier { get; set; }
    public bool AddDexterityModifierCapped { get; set; }
    public int DexterityModifierCap { get; set; }
}
