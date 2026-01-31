using Dragonwright.Database.Entities.Models;

namespace Dragonwright.Database.Entities;

public sealed class Item : IEntity<Item>
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
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsMagical { get; set; }
    
    public bool RequiresAttunement { get; set; }
    
    public bool IsConsumable { get; set; }
    
    public ItemType Type { get; set; }
    
    public Rarity Rarity { get; set; }
    
    public int WeightInOunces { get; set; }
    
    public int ValueInCopper { get; set; }
    
    public WeaponType? WeaponType { get; set; }
    
    public int? BaseArmorClass { get; set; }
    
    public int ArmorClassBonus { get; set; }
    
    public AbilityScore? ArmorClassBonusAbility { get; set; }

    public int MaximumArmorClassBonusFromAbility { get; set; }
    
    public bool GivesDisadvantageOnStealth { get; set; }
    
    public int DonningTimeInSeconds { get; set; }
    
    public int DoffingTimeInSeconds { get; set; }
    
    public AbilityScore? RequiredAbilityScore { get; set; }
    
    public int RequiredAbilityScoreValue { get; set; }
    
    public ICollection<WeaponProperty> WeaponProperties { get; set; } = [];
    
    public int RangeInFeet { get; set; }
    
    public int MaximumRangeInFeet { get; set; }
    
    public int AttackBonus { get; set; }
    
    public ICollection<AttackDamage> Damages { get; set; } = [];
    
    public AbilityScore? DamageBonusAbility { get; set; }
    
    public ICollection<DamageType> DamageTypes { get; set; } = [];
    
    public Mastery? Mastery { get; set; }
    
    public bool IsBackpack { get; set; }
    
    public Tool? ToolType { get; set; }
    
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(i => i.Source).HasConversion<string>();
        
        builder.Property(i => i.Type).HasConversion<string>();
        builder.Property(i => i.Rarity).HasConversion<string>();
        
        builder.Property(i => i.WeaponType).HasConversion<string?>();
        builder.Property(i => i.ArmorClassBonusAbility).HasConversion<string?>();
        builder.Property(i => i.RequiredAbilityScore).HasConversion<string?>();
        builder.Property(i => i.DamageBonusAbility).HasConversion<string?>();
        builder.Property(i => i.Mastery).HasConversion<string?>();
        builder.Property(i => i.ToolType).HasConversion<string?>();
        
        builder.Property(i => i.WeaponProperties).EnumCollection();
        builder.Property(i => i.DamageTypes).EnumCollection();
        builder.Property(i => i.Damages).JsonCollection();
        
        builder.HasOne(i => i.SourceCreator)
            .WithMany()
            .HasForeignKey(i => i.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}