using Dragonwright.Seeder.Models.Srd2014;

namespace Dragonwright.Seeder.Models.Srd2024;

public class SrdEquipment2024
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SrdReference>? EquipmentCategories { get; set; }
    public SrdCost? Cost { get; set; }
    public double Weight { get; set; }
    public string? Description { get; set; }
    public int? Quantity { get; set; }

    // Weapon properties
    public string? WeaponCategory { get; set; }
    public string? WeaponRange { get; set; }
    public SrdDamage? Damage { get; set; }
    public SrdRange? Range { get; set; }
    public SrdRange? ThrowRange { get; set; }
    public List<SrdReference>? Properties { get; set; }
    public SrdReference? Mastery { get; set; }
    public SrdDamage? TwoHandedDamage { get; set; }

    // Armor properties
    public SrdArmorClass2024? ArmorClass { get; set; }
    public int? StrMinimum { get; set; }
    public bool? StealthDisadvantage { get; set; }
    public string? ArmorCategory { get; set; }

    // Tool properties
    public SrdReference? Ability { get; set; }
    public List<SrdReference>? Craft { get; set; }
    public List<SrdToolUtilize>? Utilize { get; set; }

    // Storage/ammo
    public SrdReference? Storage { get; set; }

    public string Url { get; set; } = string.Empty;
}

public class SrdArmorClass2024
{
    public int Base { get; set; }
    public bool? DexBonus { get; set; }
    public int? MaxBonus { get; set; }
}

public class SrdToolUtilize
{
    public string Name { get; set; } = string.Empty;
    public SrdToolDc? Dc { get; set; }
}

public class SrdToolDc
{
    public SrdReference? DcType { get; set; }
    public int DcValue { get; set; }
    public string? SuccessType { get; set; }
}
