namespace Dragonwright.Database.Entities;

public sealed class Subclass : IEntity<Subclass>
{
    [Key]
    public Guid Id { get; set; }
    
    public SourceType Source { get; set; }
    
    public Guid? SourceCreatorId { get; set; }
    
    /// <summary>
    /// The user who created this source material.
    /// </summary>
    public User? SourceCreator { get; set; }
    
    public Guid ClassId { get; set; }
    
    public Class Class { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<Subclass> builder)
    {
        builder.Property(sc => sc.Source).HasConversion<string>();
        
        builder.HasOne(sc => sc.Class)
            .WithMany(c => c.Subclasses)
            .HasForeignKey(sc => sc.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(sc => sc.SourceCreator)
            .WithMany()
            .HasForeignKey(sc => sc.SourceCreatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}