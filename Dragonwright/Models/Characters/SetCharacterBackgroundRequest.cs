using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for setting a character's background. Pass null BackgroundId to remove the background.
/// </summary>
public sealed class SetCharacterBackgroundRequest
{
    /// <summary>
    /// The ID of the background to set. If null, removes the current background.
    /// </summary>
    public Guid? BackgroundId { get; init; }

    /// <summary>
    /// The ability score increases chosen from the background options.
    /// </summary>
    public IDictionary<AbilityScore, int> ChosenAbilityScoreIncreases { get; init; } = new Dictionary<AbilityScore, int>();

    /// <summary>
    /// The languages chosen from the background options.
    /// </summary>
    public ICollection<string> ChosenLanguages { get; init; } = [];

    /// <summary>
    /// The chosen characteristics from the background tables.
    /// </summary>
    public IDictionary<CharacteristicsType, string> ChosenCharacteristics { get; init; } = new Dictionary<CharacteristicsType, string>();
}