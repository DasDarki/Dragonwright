using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class ResistanceSubtype : ModifierSubtype
{
    public List<DamageType> DamageTypes { get; set; } = [];
    public bool AllDamage { get; set; }
    public bool NonMagicalBludgeoningPiercingSlashing { get; set; }
}
