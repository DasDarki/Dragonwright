using Dragonwright.Database.Enums;

namespace Dragonwright.Database.Entities.Modifiers;

public sealed class IgnoreSubtype : ModifierSubtype
{
    public List<Condition> Conditions { get; set; } = [];
    public List<DamageType> DamageTypes { get; set; } = [];
    public bool DifficultTerrain { get; set; }
    public bool HeavyArmorStealthDisadvantage { get; set; }
    public bool HeavyArmorStrengthRequirement { get; set; }
    public bool LoadingProperty { get; set; }
    public bool CoverBonus { get; set; }
    public bool HalfCover { get; set; }
    public bool ThreeQuartersCover { get; set; }
}
