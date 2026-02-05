using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class ClassFeatureLevelScale : IEntity<ClassFeatureLevelScale>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid ClassFeatureId { get; set; }
    
    [JsonIgnore]
    public ClassFeature ClassFeature { get; set; } = null!;
    
    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
    
    public int ClassLevel { get; set; }
    
    public int DiceCount { get; set; }
    
    public int DiceValue { get; set; }
    
    public int FixedValue { get; set; }
    
    public void Configure(EntityTypeBuilder<ClassFeatureLevelScale> builder)
    {
        builder.HasOne(cfls => cfls.ClassFeature)
            .WithMany(cf => cf.LevelScales)
            .HasForeignKey(cfls => cfls.ClassFeatureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}