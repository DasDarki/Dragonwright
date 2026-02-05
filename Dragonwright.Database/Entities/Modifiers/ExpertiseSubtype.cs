using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ExpertiseSubtype : ModifierSubtype
{
    public ProficiencyTarget Target { get; set; }
    public Skill? Skill { get; set; }
    public Tool? Tool { get; set; }
    public bool RequiresProficiency { get; set; } = true;
}
