namespace Dragonwright.Database.Enums;

public enum Proficiency
{
    NotProficient,
    HalfProficient,
    Proficient,
    Expertise
}

public static class ProficiencyExtensions
{
    public static float ToMultiplier(this Proficiency proficiency)
    {
        return proficiency switch
        {
            Proficiency.NotProficient => 0.0f,
            Proficiency.HalfProficient => 0.5f,
            Proficiency.Proficient => 1.0f,
            Proficiency.Expertise => 2.0f,
            _ => 0.0f
        };
    }
}