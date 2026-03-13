namespace Dragonwright.Database.Enums;

/// <summary>
/// Defines the visibility level of a character within a campaign.
/// </summary>
public enum CharacterVisibility
{
    /// <summary>Private — only the owner can see the character. Even the GM cannot.</summary>
    Private = 0,

    /// <summary>Campaign-private — only the owner and the GM can see the character.</summary>
    CampaignPrivate = 1,

    /// <summary>Semi-public — other players can see name and avatar, but not the sheet.</summary>
    SemiPublic = 2,

    /// <summary>Public — all campaign members can see the full sheet.</summary>
    Public = 3
}
