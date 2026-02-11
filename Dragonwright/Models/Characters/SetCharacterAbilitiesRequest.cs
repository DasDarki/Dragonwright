using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for setting all of a character's ability scores.
/// </summary>
public sealed class SetCharacterAbilitiesRequest
{
    /// <summary>
    /// The ability score data for each ability.
    /// </summary>
    public ICollection<CharacterAbilityData> Abilities { get; init; } = [];
}

/// <summary>
/// Data for a single character ability score.
/// </summary>
public sealed class CharacterAbilityData
{
    /// <summary>
    /// The ability score type.
    /// </summary>
    public AbilityScore Ability { get; init; }

    /// <summary>
    /// The raw ability score before bonuses.
    /// </summary>
    public int RawScore { get; init; }

    /// <summary>
    /// The bonus to the ability score.
    /// </summary>
    public int ScoreBonus { get; init; }

    /// <summary>
    /// The raw saving throw proficiency level.
    /// </summary>
    public Proficiency RawSavingThrowProficiency { get; init; }

    /// <summary>
    /// The overridden saving throw proficiency level.
    /// </summary>
    public Proficiency? OverrideSavingThrowProficiency { get; init; }

    /// <summary>
    /// The bonus to saving throws for this ability.
    /// </summary>
    public int SavingThrowBonus { get; init; }
}
