namespace Dragonwright.Database.Enums;

public enum Skill
{
    Acrobatics,
    AnimalHandling,
    Arcana,
    Athletics,
    Deception,
    History,
    Insight,
    Intimidation,
    Investigation,
    Medicine,
    Nature,
    Perception,
    Performance,
    Persuasion,
    Religion,
    SleightOfHand,
    Stealth,
    Survival
}

public static class SkillExtensions
{
    public static AbilityScore GetAssociatedAbility(this Skill skill)
    {
        return skill switch
        {
            Skill.Acrobatics => AbilityScore.Dexterity,
            Skill.AnimalHandling => AbilityScore.Wisdom,
            Skill.Arcana => AbilityScore.Intelligence,
            Skill.Athletics => AbilityScore.Strength,
            Skill.Deception => AbilityScore.Charisma,
            Skill.History => AbilityScore.Intelligence,
            Skill.Insight => AbilityScore.Wisdom,
            Skill.Intimidation => AbilityScore.Charisma,
            Skill.Investigation => AbilityScore.Intelligence,
            Skill.Medicine => AbilityScore.Wisdom,
            Skill.Nature => AbilityScore.Intelligence,
            Skill.Perception => AbilityScore.Wisdom,
            Skill.Performance or Skill.Persuasion => AbilityScore.Charisma,
            Skill.Religion => AbilityScore.Intelligence,
            Skill.SleightOfHand or Skill.Stealth => AbilityScore.Dexterity,
            Skill.Survival => AbilityScore.Wisdom,
            _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
        };
    }
}