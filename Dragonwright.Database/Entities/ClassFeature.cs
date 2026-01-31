namespace Dragonwright.Database.Entities;

public sealed class ClassFeature : IEntity<ClassFeature>
{
    [Key]
    public Guid Id { get; set; }
    
    public bool IsOptional { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    public string Description { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public bool HideInBuilder { get; set; }
    
    public bool HideInCharacterSheet { get; set; }
    
    public bool HasOptions { get; set; }
    
    public bool DisplayRequiredLevel { get; set; }
    
    /// <summary>
    /// If not zero, indicates the character level required to select this trait.
    /// </summary>
    public int RequiredCharacterLevel { get; set; }
    
    public FeatureType FeatureType { get; set; }
    
    public Guid? FeatureToReplaceId { get; set; }
    
    public ClassFeature? FeatureToReplace { get; set; }
    
    public ICollection<int> ClassLevelWhereOptionsArePresented { get; set; } = [];
    
    public ICollection<ClassFeatureOption> Options { get; set; } = [];
    
    public ICollection<ClassFeatureAction> Actions { get; set; } = [];
    
    public ICollection<ClassFeatureCreature> Creatures { get; set; } = [];
    
    public ICollection<ClassFeatureSpell> Spells { get; set; } = [];
    
    public ICollection<Modifier> Modifiers { get; set; } = [];
    
    public ICollection<ClassFeatureLevelScale> LevelScales { get; set; } = [];
    
    /// <summary>
    /// Additional spells added to the characters spell list by this trait.
    /// </summary>
    public ICollection<Spell> SpellList { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<ClassFeature> builder)
    {
        builder.Property(cf => cf.FeatureType).HasConversion<string>();
        
        builder.HasOne(cf => cf.FeatureToReplace)
            .WithMany()
            .HasForeignKey(cf => cf.FeatureToReplaceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(cf => cf.ClassLevelWhereOptionsArePresented).JsonCollection();
        
        builder.HasMany(rt => rt.Modifiers)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.SpellList)
            .WithMany();
    }
}