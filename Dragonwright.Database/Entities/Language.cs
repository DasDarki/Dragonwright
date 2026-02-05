namespace Dragonwright.Database.Entities;

public sealed class Language : IEntity<Language>
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a standard or exotic language.
    /// </summary>
    public LanguageType Type { get; set; }

    /// <summary>
    /// The script used to write this language (e.g., "Common", "Dwarvish", "Elvish").
    /// </summary>
    [MaxLength(100)]
    public string? Script { get; set; }

    /// <summary>
    /// Typical speakers of this language (e.g., "Dwarves", "Elves").
    /// </summary>
    public ICollection<string> TypicalSpeakers { get; set; } = [];

    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.Property(l => l.Type).HasConversion<string>();
        builder.Property(l => l.TypicalSpeakers).JsonCollection();
    }
}