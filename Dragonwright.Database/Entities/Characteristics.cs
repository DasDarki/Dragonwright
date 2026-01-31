namespace Dragonwright.Database.Entities;

public sealed class Characteristics : IEntity<Characteristics>
{
    [Key]
    public Guid Id { get; set; }
    
    public CharacteristicsType Type { get; set; }

    [MaxLength(1000)] 
    public string Text { get; set; } = string.Empty;
    
    public void Configure(EntityTypeBuilder<Characteristics> builder)
    {
        builder.Property(c => c.Type).HasConversion<string>();
    }
}