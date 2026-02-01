namespace Dragonwright.Database.Entities;

public sealed class Feat : IEntity<Feat>
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

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The feat level category (2024 rules). 0 means no level prerequisite.
    /// </summary>
    public int FeatLevel { get; set; }

    /// <summary>
    /// Whether this feat can be taken multiple times.
    /// </summary>
    public bool IsRepeatable { get; set; }

    /// <summary>
    /// Human-readable description of the feat's prerequisites.
    /// </summary>
    [MaxLength(1000)]
    public string PrerequisiteDescription { get; set; } = string.Empty;

    /// <summary>
    /// If set, the character must have at least <see cref="PrerequisiteAbilityScoreMinimum"/> in this ability score.
    /// </summary>
    public AbilityScore? PrerequisiteAbilityScore { get; set; }

    /// <summary>
    /// The minimum value required in the <see cref="PrerequisiteAbilityScore"/>.
    /// </summary>
    public int PrerequisiteAbilityScoreMinimum { get; set; }

    /// <summary>
    /// If true, the character must have spellcasting to take this feat.
    /// </summary>
    public bool PrerequisiteSpellcasting { get; set; }

    /// <summary>
    /// The ability scores the player can choose from when this feat grants an ability score increase.
    /// </summary>
    public ICollection<AbilityScore> AbilityScoreOptions { get; set; } = [];

    /// <summary>
    /// How much the chosen ability score increases (typically 1 or 2).
    /// </summary>
    public int AbilityScoreIncrease { get; set; }

    public ICollection<FeatOption> Options { get; set; } = [];

    public ICollection<FeatAction> Actions { get; set; } = [];

    public ICollection<FeatSpell> Spells { get; set; } = [];

    public ICollection<Modifier> Modifiers { get; set; } = [];

    /// <summary>
    /// Additional spells added to the character's spell list by this feat.
    /// </summary>
    public ICollection<Spell> SpellList { get; set; } = [];

    public void Configure(EntityTypeBuilder<Feat> builder)
    {
        builder.Property(f => f.Source).HasConversion<string>();
        builder.Property(f => f.PrerequisiteAbilityScore).HasConversion<string?>();
        builder.Property(f => f.AbilityScoreOptions).EnumCollection();

        builder.HasOne(f => f.SourceCreator)
            .WithMany()
            .HasForeignKey(f => f.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(f => f.Modifiers)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.SpellList)
            .WithMany();
    }
}
