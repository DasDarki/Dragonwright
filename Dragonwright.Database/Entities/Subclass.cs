using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class Subclass : IEntity<Subclass>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    public Guid? SourceCreatorId { get; set; }
    
    /// <summary>
    /// The user who created this source material.
    /// </summary>
    public User? SourceCreator { get; set; }
    
    public Guid ClassId { get; set; }
    
    [JsonIgnore]
    public Class? Class { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string ShortDescription { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool CanCastSpells { get; set; }
    
    public AbilityScore? SpellcastingAbility { get; set; }
    
    public bool KnowsAllSpells { get; set; }
    
    /// <summary>
    /// Additional class spell lists that this subclass can access.
    /// </summary>
    public ICollection<Class> AdditionalSpellLists { get; set; } = [];
    
    public SpellPrepareType? SpellPrepareType { get; set; }
    
    public SpellLearnType? SpellLearnType { get; set; }
    
    public ICollection<Spell> AdditionalSpells { get; set; } = [];
    
    public Guid? ImageId { get; set; }
    
    /// <summary>
    /// An image representing this subclass.
    /// </summary>
    public StoredFile? Image { get; set; }
    
    public ICollection<ClassFeature> ClassFeatures { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<Subclass> builder)
    {
        builder.Property(sc => sc.Source).HasConversion<string>();
        builder.Property(sc => sc.SpellcastingAbility).HasConversion<string?>();
        builder.Property(sc => sc.SpellPrepareType).HasConversion<string?>();
        builder.Property(sc => sc.SpellLearnType).HasConversion<string?>();
        
        builder.HasOne(sc => sc.Class)
            .WithMany(c => c.Subclasses)
            .HasForeignKey(sc => sc.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(sc => sc.SourceCreator)
            .WithMany()
            .HasForeignKey(sc => sc.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(sc => sc.AdditionalSpellLists)
            .WithMany();
        
        builder.HasMany(sc => sc.AdditionalSpells)
            .WithMany();

        builder.HasMany(sc => sc.ClassFeatures)
            .WithOne();
        
        builder.HasOne(u => u.Image)
            .WithMany()
            .HasForeignKey(u => u.ImageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}