using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for setting all of a character's skills.
/// </summary>
public sealed class SetCharacterSkillsRequest
{
    /// <summary>
    /// The skill data for each skill.
    /// </summary>
    public ICollection<CharacterSkillData> Skills { get; init; } = [];
}

/// <summary>
/// Data for a single character skill.
/// </summary>
public sealed class CharacterSkillData
{
    /// <summary>
    /// The skill type.
    /// </summary>
    public Skill Skill { get; init; }

    /// <summary>
    /// The bonus to the skill check.
    /// </summary>
    public int Bonus { get; init; }

    /// <summary>
    /// The raw proficiency level.
    /// </summary>
    public Proficiency RawProficiency { get; init; }

    /// <summary>
    /// The overridden proficiency level.
    /// </summary>
    public Proficiency? OverrideProficiency { get; init; }

    /// <summary>
    /// The advantage state for this skill.
    /// </summary>
    public AdvantageState AdvantageState { get; init; }
}
