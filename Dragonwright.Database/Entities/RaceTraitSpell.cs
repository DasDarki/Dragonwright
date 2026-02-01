using Dragonwright.Database.Entities.Models;

namespace Dragonwright.Database.Entities;

public sealed class RaceTraitSpell : IEntity<RaceTraitSpell>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid RaceTraitId { get; set; }
    
    public RaceTrait RaceTrait { get; set; } = null!;
    
    public Guid? SpellId { get; set; }
    
    /// <summary>
    /// If set, this spell will be granted.
    /// </summary>
    public Spell? Spell { get; set; }
    
    public Guid? ClassId { get; set; }
    
    /// <summary>
    /// If set, the character can access this class's spell list.
    /// </summary>
    public Class? Class { get; set; }

    /// <summary>
    /// If this spell is restricted to certain spell levels, this collection will contain those levels.
    /// </summary>
    public ICollection<SpellLevel> SpellLevels { get; set; } = [];
    
    /// <summary>
    /// If this spell is restricted to certain spell schools, this collection will contain those schools.
    /// </summary>
    public ICollection<SpellSchool> SpellSchools { get; set; } = [];
    
    /// <summary>
    /// If this spell is restricted to certain attack types, this collection will contain those types.
    /// </summary>
    public ICollection<AttackType> AttackTypes { get; set; } = [];
    
    /// <summary>
    /// If set to a value greater than 0, spells of level N or lower can be chosen where N is the characters level divided by this value.
    /// </summary>
    public int LevelDivisor { get; set; }
    
    public bool OnlyRitualSpells { get; set; }
    
    /// <summary>
    /// If set, the spell will use this ability score for its spellcasting.
    /// </summary>
    public AbilityScore? AbilityScore { get; set; }
    
    /// <summary>
    /// If set to a value greater than 0, this spell can be used this many times per reset type.
    /// </summary>
    public int NumberOfUses { get; set; }
    
    /// <summary>
    /// If set, this defines how NumberOfUses is modified based on a stat.
    /// </summary>
    public ArithmeticOperation? NumberOfUsesStatModifierOperation { get; set; }
    
    /// <summary>
    /// If set, this defines which stat modifies NumberOfUses.
    /// </summary>
    public AbilityScore? NumberOfUsesStatModifierAbility { get; set; }
    
    /// <summary>
    /// If true, NumberOfUses is modified by proficiency bonus using the <see cref="NumberOfUsesProficiencyOperation"/>.
    /// </summary>
    public bool NumberOfUsesProficiencyBonusIfProficient { get; set; }
    
    /// <summary>
    /// If set, this defines how NumberOfUses is modified based on proficiency.
    /// </summary>
    public ArithmeticOperation? NumberOfUsesProficiencyOperation { get; set; }
    
    /// <summary>
    /// If NumberOfUses is greater than 0, this defines the reset type for those uses. It also defines when the spell's effects reset if applicable.
    /// </summary>
    public ResetType? ResetType { get; set; }
    
    /// <summary>
    /// If set, the spell will be cast at this level.
    /// </summary>
    public SpellLevel? CastAtLevel { get; set; }
    
    /// <summary>
    /// If set, the spell will have this casting time.
    /// </summary>
    public Time? CastingTime { get; set; }
    
    /// <summary>
    /// The unit of time used to activate the spell if different from the casting time. If null, default spell casting time is used.
    /// </summary>
    public TimeUnit? ActivationTimeUnit { get; set; }
    
    /// <summary>
    /// The range in feet the spell can be cast. If null, default spell range is used.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Spell.SpellRangeSelf"/>, <see cref="Spell.SpellRangeTouch"/>, or <see cref="Spell.SpellRangeInfinite"/> for special ranges.
    /// </remarks>
    public int? Range { get; set; }

    [MaxLength(4000)] 
    public string AdditionalDescription { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Restrictions { get; set; } = string.Empty;
    
    public bool ConsumesSpellSlot { get; set; }
    
    public bool CountsAsKnownSpell { get; set; }
    
    public bool AlwaysPrepared { get; set; }
    
    /// <summary>
    /// If set to a value greater than 0, this spell becomes available when the character reaches this level.
    /// </summary>
    public int AvailableAtCharacterLevel { get; set; }
    
    /// <summary>
    /// If set to true and this is a spell list, infinite spells can be used.
    /// </summary>
    public bool IsInfinite { get; set; }
    
    public void Configure(EntityTypeBuilder<RaceTraitSpell> builder)
    {
        builder.HasOne(rto => rto.RaceTrait)
            .WithMany(rt => rt.Spells)
            .HasForeignKey(rto => rto.RaceTraitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(rto => rto.Spell)
            .WithMany()
            .HasForeignKey(rto => rto.SpellId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(rto => rto.Class)
            .WithMany()
            .HasForeignKey(rto => rto.ClassId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(rto => rto.SpellLevels).EnumCollection();
        builder.Property(rto => rto.SpellSchools).EnumCollection();
        builder.Property(rto => rto.AttackTypes).EnumCollection();
        
        builder.Property(rto => rto.AbilityScore).HasConversion<string?>();
        builder.Property(rto => rto.NumberOfUsesStatModifierAbility).HasConversion<string?>();
        builder.Property(rto => rto.NumberOfUsesStatModifierOperation).HasConversion<string?>();
        builder.Property(rto => rto.NumberOfUsesProficiencyOperation).HasConversion<string?>();
        builder.Property(rto => rto.ResetType).HasConversion<string?>();
        builder.Property(rto => rto.CastAtLevel).HasConversion<int?>();
        builder.Property(rto => rto.CastingTime).JsonValue();
        builder.Property(rto => rto.ActivationTimeUnit).HasConversion<string?>();
    }
}