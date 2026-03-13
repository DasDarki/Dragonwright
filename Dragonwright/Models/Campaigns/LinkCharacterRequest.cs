namespace Dragonwright.Models.Campaigns;

public sealed class LinkCharacterRequest
{
    /// <summary>
    /// The character ID to link. Null to unlink.
    /// </summary>
    public Guid? CharacterId { get; set; }
}
