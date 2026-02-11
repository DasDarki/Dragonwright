namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for creating a new character.
/// </summary>
public sealed class CreateCharacterRequest
{
    /// <summary>
    /// The name of the character.
    /// </summary>
    public required string Name { get; init; }
}