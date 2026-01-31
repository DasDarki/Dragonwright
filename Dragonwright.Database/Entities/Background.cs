namespace Dragonwright.Database.Entities;

public sealed class Background : IEntity<Background>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    public Guid? SourceCreatorId { get; set; }
    
    /// <summary>
    /// The user who created this source material.
    /// </summary>
    public User? SourceCreator { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public int LanguageCount { get; set; }
    
    public ICollection<AbilityScore> AbilityScoreIncreases { get; set; } = [];
    
    public ICollection<string> LanguageRestrictions { get; set; } = [];
    
    public ICollection<Skill> SkillProficiencies { get; set; } = [];
    
    public ICollection<Tool> ToolProficiencies { get; set; } = [];
    
    public ICollection<ItemType> ArmorProficiencies { get; set; } = [];
    
    public ICollection<WeaponType> WeaponProficiencies { get; set; } = [];
    
    public ICollection<Item> SpecificWeaponProficiencies { get; set; } = [];
    
    public ICollection<StartItemChoice> StartingItems { get; set; } = [];
    
    public ICollection<Feat> GrantedFeats { get; set; } = [];
    
    /// <summary>
    /// Additional spells added to the characters spell list by this trait.
    /// </summary>
    public ICollection<Spell> SpellList { get; set; } = [];

    public ICollection<Characteristics> Characteristics { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<Background> builder)
    {
        builder.Property(f => f.Source).HasConversion<string>();
        
        builder.HasOne(b => b.SourceCreator)
            .WithMany()
            .HasForeignKey(b => b.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(b => b.AbilityScoreIncreases).EnumCollection();
        builder.Property(b => b.LanguageRestrictions).JsonCollection();
        builder.Property(b => b.SkillProficiencies).EnumCollection();
        builder.Property(b => b.ToolProficiencies).EnumCollection();
        builder.Property(b => b.ArmorProficiencies).EnumCollection();
        builder.Property(b => b.WeaponProficiencies).EnumCollection();
        
        builder.HasMany(c => c.SpecificWeaponProficiencies)
            .WithMany();
        
        builder.HasMany(b => b.GrantedFeats)
            .WithMany();
        
        builder.HasMany(b => b.SpellList)
            .WithMany();
        
        builder.HasMany(c => c.StartingItems)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(b => b.Characteristics)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}