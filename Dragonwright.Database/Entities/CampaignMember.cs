namespace Dragonwright.Database.Entities;

/// <summary>
/// A player membership in a campaign, optionally linked to a character.
/// </summary>
public sealed class CampaignMember : IEntity<CampaignMember>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// The character linked to this membership. Null if no character is linked yet.
    /// </summary>
    public Guid? CharacterId { get; set; }

    public Character? Character { get; set; }

    /// <summary>
    /// The visibility level of the linked character within the campaign.
    /// </summary>
    public CharacterVisibility CharacterVisibility { get; set; } = CharacterVisibility.CampaignPrivate;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public void Configure(EntityTypeBuilder<CampaignMember> builder)
    {
        builder.HasIndex(m => new { m.CampaignId, m.UserId }).IsUnique();

        builder.Property(m => m.CharacterVisibility).HasConversion<string>();

        builder.HasOne(m => m.Campaign)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Character)
            .WithMany()
            .HasForeignKey(m => m.CharacterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
