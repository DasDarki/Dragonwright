namespace Dragonwright.Database.Entities;

public sealed class StartItemChoice : IEntity<StartItemChoice>
{
    [Key]
    public Guid Id { get; set; }
    
    public StartItemChoiceOperator Operator { get; set; }

    public ICollection<StartItem> Items { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<StartItemChoice> builder)
    {
    }
}