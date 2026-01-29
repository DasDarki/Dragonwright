namespace Dragonwright.Database.Entities;

public sealed class Race : IEntity<Race>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder.Property(r => r.Source).HasConversion<string>();
    }
}