using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for setting all of a character's classes. Replaces the entire class list.
/// </summary>
public sealed class SetCharacterClassesRequest
{
    /// <summary>
    /// The list of classes for the character. Empty list removes all classes.
    /// </summary>
    public ICollection<CharacterClassData> Classes { get; init; } = [];
}

/// <summary>
/// Data for a single character class.
/// </summary>
public sealed class CharacterClassData
{
    /// <summary>
    /// The ID of the existing CharacterClass entity, if updating. Null for new classes.
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// The ID of the class.
    /// </summary>
    public Guid ClassId { get; init; }

    /// <summary>
    /// The level in this class.
    /// </summary>
    public int Level { get; init; } = 1;

    /// <summary>
    /// The ID of the subclass (optional).
    /// </summary>
    public Guid? SubclassId { get; init; }

    /// <summary>
    /// Whether this is the starting class.
    /// </summary>
    public bool IsStartingClass { get; init; }

    /// <summary>
    /// The current usage counts for class feature abilities.
    /// </summary>
    public IDictionary<Guid, int> ClassFeatureUsages { get; init; } = new Dictionary<Guid, int>();

    /// <summary>
    /// The skill proficiencies chosen from the class options.
    /// </summary>
    public ICollection<Skill> ChosenSkillProficiencies { get; init; } = [];

    /// <summary>
    /// The options chosen for class features.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenFeatureOptions { get; init; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// The spells chosen from class feature spell lists.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenSpells { get; init; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// The spell slots used per level.
    /// </summary>
    public IDictionary<int, int> SpellSlotsUsed { get; init; } = new Dictionary<int, int>();

    /// <summary>
    /// The number of pact slots used (Warlock).
    /// </summary>
    public int PactSlotsUsed { get; init; }
}
