namespace Dragonwright.Database.Entities;

public sealed class Class : IEntity<Class>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public int HitDie { get; set; }
    
    public ICollection<AbilityScore> PrimaryAbilityScores { get; set; } = [];
    
    public ICollection<AbilityScore> SavingThrowProficiencies { get; set; } = [];
    
    /// <summary>
    /// The spell table for this class. This does not add spells to the class, but rather defines which spells are available to it.
    /// </summary>
    public ICollection<Spell> SpellTable { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.Property(c => c.Source).HasConversion<string>();

        builder.Property(c => c.PrimaryAbilityScores).EnumCollection();
        builder.Property(c => c.SavingThrowProficiencies).EnumCollection();
    }
}