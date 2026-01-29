namespace Dragonwright.Database.Entities;

public sealed class CharacterSpell : IEntity<CharacterSpell>
{
    public Guid Id { get; set; }
    
    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
    
    public Guid SpellId { get; set; }
    
    public Spell Spell { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<CharacterSpell> builder)
    {
        builder.HasOne(cs => cs.Character)
            .WithMany(c => c.Spells)
            .HasForeignKey(cs => cs.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(cs => cs.Spell)
            .WithMany()
            .HasForeignKey(cs => cs.SpellId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}