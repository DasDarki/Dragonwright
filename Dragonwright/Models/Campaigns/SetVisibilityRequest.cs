using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Campaigns;

public sealed class SetVisibilityRequest
{
    public CharacterVisibility Visibility { get; set; }
}
