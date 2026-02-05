using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ProficiencySubtype : ModifierSubtype
{
    public ProficiencyTarget Target { get; set; }
    public Skill? Skill { get; set; }
    public Tool? Tool { get; set; }
    public AbilityScore? SavingThrow { get; set; }
    public WeaponType? WeaponType { get; set; }
    public Guid? SpecificWeaponId { get; set; }
    public ItemType? ArmorType { get; set; }
}
