namespace Dragonwright.Database.Entities;

public sealed class Background : IEntity<Background>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<Background> builder)
    {
        builder.Property(f => f.Source).HasConversion<string>();
        
    }
}