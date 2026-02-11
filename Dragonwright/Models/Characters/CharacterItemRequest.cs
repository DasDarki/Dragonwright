namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for adding a character item.
/// </summary>
public sealed class AddCharacterItemRequest
{
    /// <summary>
    /// The ID of the item.
    /// </summary>
    public Guid ItemId { get; init; }

    /// <summary>
    /// The quantity of the item.
    /// </summary>
    public int Quantity { get; init; } = 1;

    /// <summary>
    /// Notes about this item instance.
    /// </summary>
    public string Notes { get; init; } = string.Empty;

    /// <summary>
    /// Whether the item is attuned.
    /// </summary>
    public bool Attuned { get; init; }

    /// <summary>
    /// Whether the item is equipped.
    /// </summary>
    public bool Equipped { get; init; }
}

/// <summary>
/// Request model for updating a character item.
/// </summary>
public sealed class UpdateCharacterItemRequest
{
    /// <summary>
    /// The quantity of the item.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Notes about this item instance.
    /// </summary>
    public string Notes { get; init; } = string.Empty;

    /// <summary>
    /// Whether the item is attuned.
    /// </summary>
    public bool Attuned { get; init; }

    /// <summary>
    /// Whether the item is equipped.
    /// </summary>
    public bool Equipped { get; init; }
}
