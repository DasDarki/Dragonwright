namespace Dragonwright.Database.Entities;

public sealed class CharacterSpell : IEntity<CharacterSpell>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Character Character { get; set; } = null!;

    public Guid SpellId { get; set; }

    public Spell Spell { get; set; } = null!;

    /// <summary>
    /// Where this spell was acquired from.
    /// </summary>
    public SpellSource SpellSource { get; set; }

    /// <summary>
    /// The class that granted this spell (relevant for multiclass characters).
    /// </summary>
    public Guid? SourceClassId { get; set; }

    public Class? SourceClass { get; set; }

    /// <summary>
    /// Whether this spell is currently prepared.
    /// </summary>
    public bool IsPrepared { get; set; }

    /// <summary>
    /// Whether this spell is always prepared (granted by a feature and cannot be unprepared).
    /// </summary>
    public bool AlwaysPrepared { get; set; }

    /// <summary>
    /// If set, the spell is cast at this level instead of its normal level (e.g., race/feat granted spells).
    /// </summary>
    public SpellLevel? CastAtLevelOverride { get; set; }

    /// <summary>
    /// Current remaining uses for limited-use spells (race/feat granted). Null means unlimited.
    /// </summary>
    public int? UsesRemaining { get; set; }

    /// <summary>
    /// Maximum uses before reset for limited-use spells. Null means unlimited.
    /// </summary>
    public int? MaxUses { get; set; }

    /// <summary>
    /// When limited uses reset.
    /// </summary>
    public ResetType? ResetType { get; set; }

    public void Configure(EntityTypeBuilder<CharacterSpell> builder)
    {
        builder.HasOne(cs => cs.Character)
            .WithMany(c => c.Spells)
            .HasForeignKey(cs => cs.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Spell)
            .WithMany()
            .HasForeignKey(cs => cs.SpellId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.SourceClass)
            .WithMany()
            .HasForeignKey(cs => cs.SourceClassId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(cs => cs.SpellSource).HasConversion<string>();
        builder.Property(cs => cs.CastAtLevelOverride).HasConversion<int?>();
        builder.Property(cs => cs.ResetType).HasConversion<string?>();
    }
}
