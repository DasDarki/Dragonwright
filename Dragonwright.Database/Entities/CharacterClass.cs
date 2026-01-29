namespace Dragonwright.Database.Entities;

public sealed class CharacterClass : IEntity<CharacterClass>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
    
    public Guid ClassId { get; set; }
    
    public Class Class { get; set; } = null!;
    
    public int Level { get; set; }
    
    public void Configure(EntityTypeBuilder<CharacterClass> builder)
    {
        builder.HasOne(cc => cc.Character)
            .WithMany(c => c.Classes)
            .HasForeignKey(cc => cc.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(cc => cc.Class)
            .WithMany()
            .HasForeignKey(cc => cc.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}