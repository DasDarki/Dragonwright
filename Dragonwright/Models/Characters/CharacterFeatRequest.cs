using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for adding or updating a character feat.
/// </summary>
public sealed class CharacterFeatRequest
{
    /// <summary>
    /// The ID of the feat.
    /// </summary>
    public Guid FeatId { get; init; }

    /// <summary>
    /// Where this feat was acquired from.
    /// </summary>
    public FeatSource Source { get; init; }

    /// <summary>
    /// The ID of the source that granted this feat.
    /// </summary>
    public Guid? SourceId { get; init; }

    /// <summary>
    /// The ability score increase chosen for this feat.
    /// </summary>
    public AbilityScore? ChosenAbilityScoreIncrease { get; init; }

    /// <summary>
    /// The options chosen for this feat.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenOptions { get; init; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// The spells chosen from this feat's spell options.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenSpells { get; init; } = new Dictionary<Guid, List<Guid>>();

    public IDictionary<Guid, int> FeatActionUsages { get; init; } = new Dictionary<Guid, int>();
}
