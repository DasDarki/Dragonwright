namespace Dragonwright.Database.Entities.Models;

public record AttackDamage
{
    public int DiceCount { get; init; }
    
    public int DiceValue { get; init; }
    
    public int Bonus { get; init; }
    
    public DamageType DamageType { get; init; }
}