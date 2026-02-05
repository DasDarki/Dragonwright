using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class RaceTraitOption : IEntity<RaceTraitOption>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid RaceTraitId { get; set; }
    
    [JsonIgnore]
    public RaceTrait RaceTrait { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    public string Description { get; set; } = string.Empty;
    
    public Guid? RequiredOptionId { get; set; }
    
    /// <summary>
    /// When set, indicates that this option requires another option to be selected first.
    /// </summary>
    public RaceTraitOption? RequiredOption { get; set; }
    
    public string RequirementDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// If not zero, indicates the character level required to select this trait option.
    /// </summary>
    public int RequiredCharacterLevel { get; set; }
    
    /// <summary>
    /// When true, indicates that this option is granted automatically and does not need to be selected.
    /// </summary>
    public bool IsGranted { get; set; }
    
    public void Configure(EntityTypeBuilder<RaceTraitOption> builder)
    {
        builder.HasOne(rto => rto.RaceTrait)
            .WithMany(rt => rt.Options)
            .HasForeignKey(rto => rto.RaceTraitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(rto => rto.RequiredOption)
            .WithMany()
            .HasForeignKey(rto => rto.RequiredOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}