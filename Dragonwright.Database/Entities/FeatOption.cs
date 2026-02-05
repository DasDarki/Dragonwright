using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class FeatOption : IEntity<FeatOption>
{
    [Key]
    public Guid Id { get; set; }

    public Guid FeatId { get; set; }

    [JsonIgnore]
    public Feat Feat { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public Guid? RequiredOptionId { get; set; }

    /// <summary>
    /// When set, indicates that this option requires another option to be selected first.
    /// </summary>
    public FeatOption? RequiredOption { get; set; }

    public string RequirementDescription { get; set; } = string.Empty;

    /// <summary>
    /// If not zero, indicates the character level required to select this feat option.
    /// </summary>
    public int RequiredCharacterLevel { get; set; }

    /// <summary>
    /// When true, indicates that this option is granted automatically and does not need to be selected.
    /// </summary>
    public bool IsGranted { get; set; }

    public void Configure(EntityTypeBuilder<FeatOption> builder)
    {
        builder.HasOne(fo => fo.Feat)
            .WithMany(f => f.Options)
            .HasForeignKey(fo => fo.FeatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fo => fo.RequiredOption)
            .WithMany()
            .HasForeignKey(fo => fo.RequiredOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
