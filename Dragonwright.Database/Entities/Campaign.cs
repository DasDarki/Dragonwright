namespace Dragonwright.Database.Entities;

/// <summary>
/// A campaign run by a game master that players can join with their characters.
/// </summary>
public sealed class Campaign : IEntity<Campaign>
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public Guid GameMasterId { get; set; }

    public User GameMaster { get; set; } = null!;

    /// <summary>
    /// Short unique code used for invite links.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string InviteCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CampaignMember> Members { get; set; } = [];

    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasIndex(c => c.InviteCode).IsUnique();

        builder.HasOne(c => c.GameMaster)
            .WithMany()
            .HasForeignKey(c => c.GameMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
