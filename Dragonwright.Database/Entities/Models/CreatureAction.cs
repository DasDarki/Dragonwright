namespace Dragonwright.Database.Entities.Models;

public record CreatureAction
{
    public string Name { get; init; } = string.Empty;
    
    public string Description { get; init; } = string.Empty;
    
    public IList<AttackType>? AttackTypes { get; init; }
    
    public IList<DamageType>? DamageTypes { get; init; }
    
    public int? AttackRoll { get; init; }
    
    public IList<AttackDamage>? Damages { get; init; }
}