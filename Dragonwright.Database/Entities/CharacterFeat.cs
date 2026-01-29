namespace Dragonwright.Database.Entities;

public sealed class CharacterFeat : IEntity<CharacterFeat>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;

    public Guid FeatId { get; set; }
    
    public Feat Feat { get; set; } = null!;
    
    public FeatSource Source { get; set; }
    
    public Guid? SourceId { get; set; }
    
    public void Configure(EntityTypeBuilder<CharacterFeat> builder)
    {
        builder.HasOne(cf => cf.Character)
            .WithMany(c => c.Feats)
            .HasForeignKey(cf => cf.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(cf => cf.Feat)
            .WithMany()
            .HasForeignKey(cf => cf.FeatId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(cf => cf.Source).HasConversion<string>();
    }
}