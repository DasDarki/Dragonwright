namespace Dragonwright.Seeder.Models.Srd2014;

public class SrdEquipment
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SrdReference? EquipmentCategory { get; set; }
    public string? WeaponCategory { get; set; }
    public string? WeaponRange { get; set; }
    public SrdCost? Cost { get; set; }
    public SrdDamage? Damage { get; set; }
    public SrdRange? Range { get; set; }
    public double Weight { get; set; }
    public List<SrdReference> Properties { get; set; } = [];
    public SrdRange? ThrowRange { get; set; }
    public SrdArmorClass? ArmorClass { get; set; }
    public int? StrMinimum { get; set; }
    public bool? StealthDisadvantage { get; set; }
    public string? ArmorCategory { get; set; }
    public List<string>? Desc { get; set; }
    public string? ToolCategory { get; set; }
    public string? VehicleCategory { get; set; }
}

public class SrdCost
{
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class SrdDamage
{
    public string DamageDice { get; set; } = string.Empty;
    public SrdReference? DamageType { get; set; }
}

public class SrdRange
{
    public int? Normal { get; set; }
    public int? Long { get; set; }
}

public class SrdArmorClass
{
    public int Base { get; set; }
    public bool? DexBonus { get; set; }
    public int? MaxBonus { get; set; }
}

public class SrdEquipmentContent
{
    public SrdReference? Item { get; set; }
    public int Quantity { get; set; }
}

public class SrdMagicItem
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SrdReference? EquipmentCategory { get; set; }
    public SrdReference? Rarity { get; set; }
    public List<SrdReference>? Variants { get; set; }
    public bool? Variant { get; set; }
    public List<string>? Desc { get; set; }
    public string Url { get; set; } = string.Empty;
}
