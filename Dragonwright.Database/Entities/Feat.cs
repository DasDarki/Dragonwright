namespace Dragonwright.Database.Entities;

public sealed class Feat : IEntity<Feat>
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
    
    public void Configure(EntityTypeBuilder<Feat> builder)
    {
        builder.Property(f => f.Source).HasConversion<string>();
        
        builder.HasOne(f => f.SourceCreator)
            .WithMany()
            .HasForeignKey(f => f.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}