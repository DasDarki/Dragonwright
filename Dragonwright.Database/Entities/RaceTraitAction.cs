using Dragonwright.Database.Entities.Models;
using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class RaceTraitAction : IEntity<RaceTraitAction>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid RaceTraitId { get; set; }
    
    [JsonIgnore]
    public RaceTrait? RaceTrait { get; set; }
    
    public ActionType ActionType { get; set; }
    
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public AbilityScore? AbilityScore { get; set; }
    
    public int RequiredCharacterLevel { get; set; }
    
    /// <summary>
    /// Whether the character's proficiency bonus applies to this action.
    /// </summary>
    public bool IsProficient { get; set; }
    
    /// <summary>
    /// If this action involves an attack, the type of attack.
    /// </summary>
    public AttackType? AttackType { get; set; }
    
    /// <summary>
    /// If this action involves a saving throw, the ability score used for the save.
    /// </summary>
    public AbilityScore? Save { get; set; }
    
    /// <summary>
    /// If this action involves a saving throw, the fixed DC for the save. If 0, the DC is calculated normally.
    /// </summary>
    public int FixedSaveDC { get; set; }
    
    public int DiceCount { get; set; }
    
    public int DiceValue { get; set; }
    
    public int FixedValue { get; set; }
    
    [MaxLength(4000)]
    public string EffectOnMiss { get; set; } = string.Empty;
    
    [MaxLength(4000)]
    public string EffectOnSaveSuccess { get; set; } = string.Empty;
    
    [MaxLength(4000)]
    public string EffectOnSaveFailure { get; set; } = string.Empty;
    
    public bool IsUnarmedWeapon { get; set; }
    
    public bool IsNaturalWeapon { get; set; }
    
    public DamageType? DamageType { get; set; }
    
    public bool DisplayAsAttack { get; set; }
    
    /// <summary>
    /// True, if this action is affected by the Monk's Martial Arts feature.
    /// </summary>
    public bool EffectByMartialArts { get; set; }
    
    /// <summary>
    /// The range in feet the action uses.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Spell.SpellRangeSelf"/>, <see cref="Spell.SpellRangeTouch"/>, or <see cref="Spell.SpellRangeInfinite"/> for special ranges.
    /// </remarks>
    public int? Range { get; set; }
    
    /// <summary>
    /// The maximum/long range in feet the action uses.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Spell.SpellRangeSelf"/>, <see cref="Spell.SpellRangeTouch"/>, or <see cref="Spell.SpellRangeInfinite"/> for special ranges.
    /// </remarks>
    public int? MaximumRange { get; set; }
    
    public Shape? AreaOfEffect { get; set; }
    
    public int AreaSize { get; set; }
    
    /// <summary>
    /// The time used to activate the action.
    /// </summary>
    public Time? ActivationTime { get; set; }
    
    /// <summary>
    /// The reset type for this action if it has limited uses.
    /// </summary>
    public ResetType? ResetType { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public void Configure(EntityTypeBuilder<RaceTraitAction> builder)
    {
        builder.HasOne(rto => rto.RaceTrait)
            .WithMany(rt => rt.Actions)
            .HasForeignKey(rto => rto.RaceTraitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(rta => rta.ActionType).HasConversion<string>();
        builder.Property(rta => rta.AttackType).HasConversion<string?>();
        builder.Property(rta => rta.AbilityScore).HasConversion<string?>();
        builder.Property(rta => rta.Save).HasConversion<string?>();
        builder.Property(rta => rta.DamageType).HasConversion<string?>();
        builder.Property(rta => rta.AreaOfEffect).HasConversion<string?>();
        builder.Property(rta => rta.ResetType).HasConversion<string?>();
        builder.Property(rta => rta.ActivationTime).JsonValue();
    }
}