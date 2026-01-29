namespace Dragonwright.Database.Entities;

public sealed class Feat : IEntity<Feat>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<Feat> builder)
    {
        builder.Property(f => f.Source).HasConversion<string>();
        
    }
}