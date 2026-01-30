namespace Dragonwright.Database.Entities;

public sealed class ClassFeature : IEntity<ClassFeature>
{
    [Key]
    public Guid Id { get; set; }
    
    public void Configure(EntityTypeBuilder<ClassFeature> builder)
    {
        
    }
}