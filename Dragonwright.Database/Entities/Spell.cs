using Dragonwright.Database.Entities.Models;

namespace Dragonwright.Database.Entities;

public sealed class Spell : IEntity<Spell>
{
    public const int SpellRangeSelf = 0;
    public const int SpellRangeTouch = -1;
    public const int SpellRangeInfinite = -2;
    
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    public Guid? SourceCreatorId { get; set; }
    
    /// <summary>
    /// The user who created this source material.
    /// </summary>
    public User? SourceCreator { get; set; }
    
    [Required]
    public SpellLevel Level { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    [MaxLength(4000)]
    public string Description { get; set; } = null!;
    
    public SpellSchool School { get; set; } = SpellSchool.Unspecified;

    public ICollection<Time> CastingTimes { get; set; } = [];
    
    public bool HasVocalComponent { get; set; }
    
    public bool HasSomaticComponent { get; set; }
    
    public bool HasMaterialComponent { get; set; }
    
    [MaxLength(500)]
    public string? MaterialComponents { get; set; }
    
    public bool Concentration { get; set; }
    
    public bool Ritual { get; set; }
    
    public AttackType AttackType { get; set; } = AttackType.None;
    
    public AbilityScore? Save { get; set; }
    
    /// <summary>
    /// The range in feet the spell can be cast.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SpellRangeSelf"/>, <see cref="SpellRangeTouch"/>, or <see cref="SpellRangeInfinite"/> for special ranges.
    /// </remarks>
    public int Range { get; set; }
    
    public Shape? AreaOfEffect { get; set; }
    
    public int AreaSize { get; set; }
    
    public ICollection<DamageType> DamageTypes { get; set; } = [];
    
    public ICollection<Condition> Conditions { get; set; } = [];
    
    public ICollection<Time> Durations { get; set; } = [];

    public ICollection<string> Tags { get; set; } = [];
    
    /// <summary>
    /// Classes to which this spell belongs - it basically defines their spell table.
    /// </summary>
    public ICollection<Class> Classes { get; set; } = [];
    
    
    public void Configure(EntityTypeBuilder<Spell> builder)
    {
        builder.Property(s => s.Source).HasConversion<string>();
        
        builder.Property(s => s.Level).HasConversion<int>();
        builder.Property(s => s.School).HasConversion<string>();
        builder.Property(s => s.AttackType).HasConversion<string>();
        
        builder.Property(s => s.DamageTypes).EnumCollection();
        builder.Property(s => s.Conditions).EnumCollection();
        builder.Property(s => s.CastingTimes).JsonCollection();
        builder.Property(s => s.Durations).JsonCollection();
        builder.Property(s => s.Tags).JsonCollection();

        builder.HasMany(s => s.Classes)
            .WithMany(c => c.SpellList);
        
        builder.HasOne(s => s.SourceCreator)
            .WithMany()
            .HasForeignKey(s => s.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}