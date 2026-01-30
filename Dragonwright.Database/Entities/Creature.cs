namespace Dragonwright.Database.Entities;

public sealed class Creature : IEntity<Creature>
{
    [Key]
    public Guid Id { get; set; }
    
    public void Configure(EntityTypeBuilder<Creature> builder)
    {
        
    }
}