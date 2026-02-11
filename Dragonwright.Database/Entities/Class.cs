namespace Dragonwright.Database.Entities;

public sealed class Class : IEntity<Class>
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

    public int HitDie { get; set; }
    
    public int FixHitPointsPerLevelAfterFirst { get; set; }
    
    public int BaseHitPointsAtFirstLevel { get; set; }
    
    public AbilityScore? HitPointsModifierAbilityScore { get; set; }
    
    public ICollection<AbilityScore> PrimaryAbilityScores { get; set; } = [];
    
    public ICollection<AbilityScore> SavingThrowProficiencies { get; set; } = [];
    
    public ICollection<ClassFeature> Features { get; set; } = [];
    
    /// <summary>
    /// The spell table for this class. This does not add spells to the class, but rather defines which spells are available to it.
    /// </summary>
    public ICollection<Spell> SpellList { get; set; } = [];
    
    public ICollection<Subclass> Subclasses { get; set; } = [];
    
    public int SkillProficienciesCount { get; set; }
    
    public ICollection<Skill> SkillProficienciesOptions { get; set; } = [];
    
    public ICollection<Tool> ToolProficiencies { get; set; } = [];
    
    public ICollection<ItemType> ArmorProficiencies { get; set; } = [];
    
    public ICollection<WeaponType> WeaponProficiencies { get; set; } = [];
    
    public ICollection<Item> SpecificWeaponProficiencies { get; set; } = [];
    
    public ICollection<StartItemChoice> StartingItems { get; set; } = [];
    
    public Guid? ImageId { get; set; }

    public StoredFile? Image { get; set; }

    /// <summary>
    /// The standard array values for ability score generation.
    /// Typically [15, 14, 13, 12, 10, 8] for most classes.
    /// </summary>
    public ICollection<int> StandardArray { get; set; } = [15, 14, 13, 12, 10, 8];

    /// <summary>
    /// Minimum ability scores required to multiclass INTO this class.
    /// Maps AbilityScore enum to minimum value (typically 13).
    /// All requirements must be met (AND logic).
    /// Example: Paladin requires {Strength: 13, Charisma: 13}
    /// </summary>
    public IDictionary<AbilityScore, int> MulticlassingRequirements { get; set; } = new Dictionary<AbilityScore, int>();

    /// <summary>
    /// Alternative multiclassing requirements (OR logic with primary requirements).
    /// If EITHER MulticlassingRequirements OR MulticlassingRequirementsAlt is satisfied, allow multiclass.
    /// Example: Fighter can use {Strength: 13} OR {Dexterity: 13}
    /// </summary>
    public IDictionary<AbilityScore, int> MulticlassingRequirementsAlt { get; set; } = new Dictionary<AbilityScore, int>();

    /// <summary>
    /// The class level at which a subclass must be chosen.
    /// 0 = no subclass, 1 = Cleric/Warlock, 2 = Wizard, 3 = most classes.
    /// </summary>
    public int SubclassSelectionLevel { get; set; } = 3;

    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.Property(c => c.Source).HasConversion<string>();

        builder.Property(c => c.PrimaryAbilityScores).EnumCollection();
        builder.Property(c => c.SavingThrowProficiencies).EnumCollection();
        
        builder.HasMany(c => c.Features)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(c => c.SourceCreator)
            .WithMany()
            .HasForeignKey(c => c.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.SkillProficienciesOptions).EnumCollection();
        builder.Property(c => c.ToolProficiencies).EnumCollection();
        builder.Property(c => c.ArmorProficiencies).EnumCollection();
        builder.Property(c => c.WeaponProficiencies).EnumCollection();
        
        builder.HasMany(c => c.SpecificWeaponProficiencies)
            .WithMany();
        
        builder.HasMany(c => c.StartingItems)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(u => u.Image)
            .WithMany()
            .HasForeignKey(u => u.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.StandardArray).JsonCollection();
        builder.Property(c => c.MulticlassingRequirements).EnumKeyDictionary();
        builder.Property(c => c.MulticlassingRequirementsAlt).EnumKeyDictionary();
    }
}