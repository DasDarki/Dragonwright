using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for adding a character spell.
/// </summary>
public sealed class AddCharacterSpellRequest
{
    /// <summary>
    /// The ID of the spell.
    /// </summary>
    public Guid SpellId { get; init; }

    /// <summary>
    /// Where this spell was acquired from.
    /// </summary>
    public SpellSource SpellSource { get; init; }

    /// <summary>
    /// The ID of the class that granted this spell.
    /// </summary>
    public Guid? SourceClassId { get; init; }

    /// <summary>
    /// Whether this spell is currently prepared.
    /// </summary>
    public bool IsPrepared { get; init; }

    /// <summary>
    /// Whether this spell is always prepared.
    /// </summary>
    public bool AlwaysPrepared { get; init; }

    /// <summary>
    /// The level at which to cast this spell.
    /// </summary>
    public SpellLevel? CastAtLevelOverride { get; init; }

    /// <summary>
    /// The maximum uses for limited-use spells.
    /// </summary>
    public int? MaxUses { get; init; }

    /// <summary>
    /// When limited uses reset.
    /// </summary>
    public ResetType? ResetType { get; init; }
}

/// <summary>
/// Request model for updating a character spell.
/// </summary>
public sealed class UpdateCharacterSpellRequest
{
    /// <summary>
    /// Whether this spell is currently prepared.
    /// </summary>
    public bool IsPrepared { get; init; }

    /// <summary>
    /// The remaining uses for limited-use spells.
    /// </summary>
    public int? UsesRemaining { get; init; }
}
