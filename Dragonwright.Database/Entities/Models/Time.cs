namespace Dragonwright.Database.Entities.Models;

public record Time
{
    public static readonly Time Instant = new() { Value = 0, Unit = TimeUnit.Instantaneous };
    
    public int Value { get; init; }
    
    public TimeUnit Unit { get; init; }
}