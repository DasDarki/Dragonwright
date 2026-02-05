namespace Dragonwright.Database.Entities;

public sealed class Language : IEntity<Language>
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
    
    public void Configure(EntityTypeBuilder<Language> builder)
    {
    }
}