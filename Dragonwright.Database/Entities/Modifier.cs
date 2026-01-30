using Dragonwright.Database.Entities.Models;
using Dragonwright.Database.Entities.Modifiers;

namespace Dragonwright.Database.Entities;

public sealed class Modifier : IEntity<Modifier>
{
    [Key]
    public Guid Id { get; set; }
    
    public ModifierType Type { get; set; }
    
    public ModifierSubtype? Subtype { get; set; } 
    
    public AbilityScore? AbilityScore { get; set; }
    
    public int DiceCount { get; set; }
    
    public int DiceValue { get; set; }
    
    public int FixedValue { get; set; }
    
    [MaxLength(4000)]
    public string Details { get; set; } = string.Empty;
    
    public Time? Duration { get; set; }
    
    public void Configure(EntityTypeBuilder<Modifier> builder)
    {
        builder.Property(m => m.Type).HasConversion<string>();
        builder.Property(m => m.AbilityScore).HasConversion<string?>();
        
        builder.Property(m => m.Subtype).JsonValue();
        builder.Property(m => m.Duration).JsonValue();

    }
}