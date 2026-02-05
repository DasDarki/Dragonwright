namespace Dragonwright.Database.Entities.Modifiers;

public sealed class AdvantageSubtype : ModifierSubtype
{
    public RollTarget Target { get; set; }
    public AbilityScore? AbilityScore { get; set; }
    public Skill? Skill { get; set; }
    public CreatureType? AgainstCreatureType { get; set; }
    public Condition? WhenAfflictedByCondition { get; set; }
    public bool WhenHidden { get; set; }
    public bool OnFirstAttackPerTurn { get; set; }
}
