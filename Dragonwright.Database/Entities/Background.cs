namespace Dragonwright.Database.Entities;

public sealed class Background : IEntity<Background>
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
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<Background> builder)
    {
        builder.Property(f => f.Source).HasConversion<string>();
        
        builder.HasOne(b => b.SourceCreator)
            .WithMany()
            .HasForeignKey(b => b.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}