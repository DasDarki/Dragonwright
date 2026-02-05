using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ImmunitySubtype : ModifierSubtype
{
    public List<DamageType> DamageTypes { get; set; } = [];
    public List<Condition> Conditions { get; set; } = [];
    public bool Disease { get; set; }
    public bool Poison { get; set; }
    public bool MagicalSleep { get; set; }
}
