using System.ComponentModel.DataAnnotations;

namespace Dragonwright.Models.Campaigns;

public sealed class JoinCampaignRequest
{
    [Required]
    public string InviteCode { get; set; } = null!;
}
