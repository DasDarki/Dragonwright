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
    
    public ICollection<AbilityScore> PrimaryAbilityScores { get; set; } = [];
    
    public ICollection<AbilityScore> SavingThrowProficiencies { get; set; } = [];
    
    public ICollection<ClassFeature> Features { get; set; } = [];
    
    /// <summary>
    /// The spell table for this class. This does not add spells to the class, but rather defines which spells are available to it.
    /// </summary>
    public ICollection<Spell> SpellList { get; set; } = [];
    
    public ICollection<Subclass> Subclasses { get; set; } = [];
    
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
    }
}