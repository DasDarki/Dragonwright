using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class DisadvantageSubtype : ModifierSubtype
{
    public RollTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public Skill? Skill { get; set; }
    public bool WhenWithinMeleeRange { get; set; }
    public bool WhenInSunlight { get; set; }
}
