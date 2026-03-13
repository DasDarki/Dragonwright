using System.ComponentModel.DataAnnotations;

namespace Dragonwright.Models.Campaigns;

public sealed class CreateCampaignRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}
