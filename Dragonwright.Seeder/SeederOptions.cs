namespace Dragonwright.Seeder;

public class SeederOptions
{
    public bool ShowHelp { get; set; }
    public bool Seed2014 { get; set; }
    public bool Seed2024 { get; set; }
    public bool Clean { get; set; }
    public string? EntityFilter { get; set; }

    public bool ShouldSeedEntity(string entityType)
    {
        if (string.IsNullOrEmpty(EntityFilter))
            return true;

        return EntityFilter.Equals(entityType, StringComparison.OrdinalIgnoreCase) ||
               EntityFilter.Equals(entityType + "s", StringComparison.OrdinalIgnoreCase) ||
               (EntityFilter + "s").Equals(entityType, StringComparison.OrdinalIgnoreCase);
    }
}
