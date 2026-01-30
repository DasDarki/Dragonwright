namespace Dragonwright.Database.Entities;

public sealed class Race : IEntity<Race>
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
    
    public CreatureType Type { get; set; }
    
    public ICollection<RaceTrait> Traits { get; set; } = [];
    
    public ICollection<RaceVariant> Variants { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder.Property(r => r.Source).HasConversion<string>();
        builder.Property(r => r.Type).HasConversion<string>();
        
        builder.HasMany(r => r.Traits)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(r => r.SourceCreator)
            .WithMany()
            .HasForeignKey(r => r.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}