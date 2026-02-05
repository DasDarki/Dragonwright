using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class HalfProficiencySubtype : ModifierSubtype
{
    public ProficiencyTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public Skill? Skill { get; set; }
    public Tool? Tool { get; set; }
    public bool AllSkills { get; set; }
    public bool AllAbilityChecks { get; set; }
}
