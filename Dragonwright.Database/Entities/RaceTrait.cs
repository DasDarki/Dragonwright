using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class RaceTrait : IEntity<RaceTrait>
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    public string Description { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public bool HideInBuilder { get; set; }
    
    public bool HideInCharacterSheet { get; set; }
    
    public FeatureType FeatureType { get; set; }
    
    public Guid? TraitToReplaceId { get; set; }
    
    public RaceTrait? TraitToReplace { get; set; }
    
    public ICollection<int> CharactersLevelWhereOptionsArePresented { get; set; } = [];
    
    /// <summary>
    /// If not zero, indicates the character level required to select this trait.
    /// </summary>
    public int RequiredCharacterLevel { get; set; }
    
    public ICollection<RaceTraitOption> Options { get; set; } = [];
    
    public ICollection<RaceTraitAction> Actions { get; set; } = [];
    
    public ICollection<RaceTraitCreature> Creatures { get; set; } = [];
    
    public ICollection<RaceTraitSpell> Spells { get; set; } = [];
    
    public ICollection<Modifier> Modifiers { get; set; } = [];
    
    /// <summary>
    /// Additional spells added to the characters spell list by this trait.
    /// </summary>
    public ICollection<Spell> SpellList { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<RaceTrait> builder)
    {
        builder.Property(rt => rt.FeatureType).HasConversion<string>();
        
        builder.HasOne(rt => rt.TraitToReplace)
            .WithMany()
            .HasForeignKey(rt => rt.TraitToReplaceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(rt => rt.CharactersLevelWhereOptionsArePresented).JsonCollection();
        
        builder.HasMany(rt => rt.Options)
            .WithOne(rto => rto.RaceTrait)
            .HasForeignKey(rto => rto.RaceTraitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(rt => rt.Modifiers)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.SpellList)
            .WithMany();
    }
}