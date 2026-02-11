namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for setting a character's race. Pass null to remove the race.
/// </summary>
public sealed class SetCharacterRaceRequest
{
    /// <summary>
    /// The ID of the race to set. If null, removes the current race.
    /// </summary>
    public Guid? RaceId { get; init; }

    /// <summary>
    /// The current usage counts for race trait abilities.
    /// </summary>
    public IDictionary<Guid, int> RaceTraitUsages { get; init; } = new Dictionary<Guid, int>();

    /// <summary>
    /// The options chosen for race traits.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenTraitOptions { get; init; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// The spells chosen from race trait spell lists.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenSpells { get; init; } = new Dictionary<Guid, List<Guid>>();
}
