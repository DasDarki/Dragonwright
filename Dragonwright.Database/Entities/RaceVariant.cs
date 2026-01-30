namespace Dragonwright.Database.Entities;

public sealed class RaceVariant : IEntity<RaceVariant>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid RaceId { get; set; }
    
    public Race Race { get; set; } = null!;
    
    public void Configure(EntityTypeBuilder<RaceVariant> builder)
    {
        builder.HasOne(rv => rv.Race)
            .WithMany(r => r.Variants)
            .HasForeignKey(rv => rv.RaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}