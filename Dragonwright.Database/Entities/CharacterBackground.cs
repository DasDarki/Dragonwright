namespace Dragonwright.Database.Entities;

public sealed class CharacterBackground : IEntity<CharacterBackground>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
    
    public Guid BackgroundId { get; set; }
    
    public Background Background { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<CharacterBackground> builder)
    {
        builder.HasOne(cb => cb.Character)
            .WithOne(c => c.Background)
            .HasForeignKey<CharacterBackground>(cb => cb.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(cb => cb.Background)
            .WithMany()
            .HasForeignKey(cb => cb.BackgroundId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}