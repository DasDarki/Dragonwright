namespace Dragonwright.Database.Entities;

public sealed class CharacterRace : IEntity<CharacterRace>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
    
    public Guid RaceId { get; set; }
    
    public Race Race { get; set; } = null!;
    
    public IDictionary<Guid, int> RaceTraitUsages { get; set; } = new Dictionary<Guid, int>();
    
    public void Configure(EntityTypeBuilder<CharacterRace> builder)
    {
        builder.HasOne(cr => cr.Character)
            .WithOne(c => c.Race)
            .HasForeignKey<CharacterRace>(cr => cr.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(cr => cr.Race)
            .WithMany()
            .HasForeignKey(cr => cr.RaceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(cr => cr.RaceTraitUsages).JsonDictionary();
    }
}