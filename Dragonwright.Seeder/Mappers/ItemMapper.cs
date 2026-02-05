using Dragonwright.Database.Entities.Models;
using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Models.Srd2024;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class ItemMapper
{
    public static Item? Map(SrdEquipment srd, SourceType source, IndexLookup lookup)
    {
        var categoryIndex = srd.EquipmentCategory?.Index?.ToLowerInvariant() ?? "";

        // Skip packs (they're bundles, not items)
        if (categoryIndex == "equipment-packs") return null;

        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Items[key] = id;

        var item = new Item
        {
            Id = id,
            Source = source,
            Name = srd.Name,
            Description = srd.Desc != null ? string.Join("\n", srd.Desc) : string.Empty,
            Type = DetermineItemType(srd),
            ValueInCopper = srd.Cost != null
                ? MapperHelpers.CostToCopper(srd.Cost.Quantity, srd.Cost.Unit)
                : 0,
            WeightInOunces = MapperHelpers.WeightToOunces(srd.Weight),
            Rarity = Rarity.Common
        };

        // Map weapon properties
        if (categoryIndex == "weapon" || !string.IsNullOrEmpty(srd.WeaponCategory))
        {
            MapWeaponProperties(srd, item);
        }

        // Map armor properties
        if (categoryIndex == "armor" || !string.IsNullOrEmpty(srd.ArmorCategory))
        {
            MapArmorProperties(srd, item);
        }

        // Map tool type
        if (categoryIndex == "tools" || !string.IsNullOrEmpty(srd.ToolCategory))
        {
            item.ToolType = MapperHelpers.ParseTool(srd.Index);
        }

        return item;
    }

    public static Item? Map(SrdEquipment2024 srd, IndexLookup lookup)
    {
        var categories = srd.EquipmentCategories?.Select(c => c.Index.ToLowerInvariant()).ToList() ?? [];

        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, SourceType.One2024);
        lookup.Items[key] = id;

        var item = new Item
        {
            Id = id,
            Source = SourceType.One2024,
            Name = srd.Name,
            Description = srd.Description ?? string.Empty,
            Type = DetermineItemType2024(srd, categories),
            ValueInCopper = srd.Cost != null
                ? MapperHelpers.CostToCopper(srd.Cost.Quantity, srd.Cost.Unit)
                : 0,
            WeightInOunces = MapperHelpers.WeightToOunces(srd.Weight),
            Rarity = Rarity.Common
        };

        // Map weapon properties
        if (categories.Contains("weapon") || !string.IsNullOrEmpty(srd.WeaponCategory))
        {
            MapWeaponProperties2024(srd, item);
        }

        // Map armor properties
        if (categories.Contains("armor") || !string.IsNullOrEmpty(srd.ArmorCategory))
        {
            MapArmorProperties2024(srd, item);
        }

        // Map tool type
        if (categories.Contains("tools") || categories.Contains("artisans-tools"))
        {
            item.ToolType = MapperHelpers.ParseTool(srd.Index);
        }

        return item;
    }

    private static ItemType DetermineItemType(SrdEquipment srd)
    {
        var categoryIndex = srd.EquipmentCategory?.Index?.ToLowerInvariant() ?? "";
        var armorCategory = srd.ArmorCategory?.ToLowerInvariant() ?? "";

        if (categoryIndex == "weapon" || !string.IsNullOrEmpty(srd.WeaponCategory))
            return ItemType.Weapon;

        if (categoryIndex == "armor" || !string.IsNullOrEmpty(srd.ArmorCategory))
        {
            return armorCategory switch
            {
                "light" => ItemType.LightArmor,
                "medium" => ItemType.MediumArmor,
                "heavy" => ItemType.HeavyArmor,
                "shield" => ItemType.Shield,
                _ => ItemType.LightArmor
            };
        }

        if (categoryIndex == "tools" || !string.IsNullOrEmpty(srd.ToolCategory))
            return ItemType.Tool;

        if (categoryIndex == "mounts-and-vehicles" || !string.IsNullOrEmpty(srd.VehicleCategory))
        {
            if (srd.VehicleCategory?.ToLowerInvariant().Contains("mount") == true)
                return ItemType.Mount;
            return ItemType.Vehicle;
        }

        return ItemType.Other;
    }

    private static ItemType DetermineItemType2024(SrdEquipment2024 srd, List<string> categories)
    {
        if (categories.Contains("weapon") || !string.IsNullOrEmpty(srd.WeaponCategory))
            return ItemType.Weapon;

        if (categories.Contains("armor") || !string.IsNullOrEmpty(srd.ArmorCategory))
        {
            var armorCategory = srd.ArmorCategory?.ToLowerInvariant() ?? "";
            return armorCategory switch
            {
                "light" => ItemType.LightArmor,
                "medium" => ItemType.MediumArmor,
                "heavy" => ItemType.HeavyArmor,
                "shield" => ItemType.Shield,
                _ => ItemType.LightArmor
            };
        }

        if (categories.Contains("tools") || categories.Contains("artisans-tools"))
            return ItemType.Tool;

        if (categories.Contains("ammunition"))
            return ItemType.Ammunition;

        return ItemType.Other;
    }

    private static void MapWeaponProperties(SrdEquipment srd, Item item)
    {
        item.Type = ItemType.Weapon;

        // Parse weapon type
        var weaponCat = srd.WeaponCategory?.ToLowerInvariant() ?? "";
        var weaponRange = srd.WeaponRange?.ToLowerInvariant() ?? "";

        item.WeaponType = (weaponCat, weaponRange) switch
        {
            ("simple", "melee") => WeaponType.SimpleMelee,
            ("simple", "ranged") => WeaponType.SimpleRanged,
            ("martial", "melee") => WeaponType.MartialMelee,
            ("martial", "ranged") => WeaponType.MartialRanged,
            _ => WeaponType.SimpleMelee
        };

        // Parse damage
        if (srd.Damage != null)
        {
            var (diceCount, diceValue) = MapperHelpers.ParseDamageDice(srd.Damage.DamageDice);
            var damageType = MapperHelpers.ParseDamageType(srd.Damage.DamageType?.Index);

            if (damageType.HasValue)
            {
                item.Damages.Add(new AttackDamage
                {
                    DiceCount = diceCount,
                    DiceValue = diceValue,
                    Bonus = 0,
                    DamageType = damageType.Value
                });
                item.DamageTypes.Add(damageType.Value);
            }
        }

        // Parse range
        if (srd.Range?.Normal.HasValue == true)
        {
            item.RangeInFeet = srd.Range.Normal.Value;
            item.MaximumRangeInFeet = srd.Range.Long ?? srd.Range.Normal.Value;
        }

        // Parse throw range
        if (srd.ThrowRange != null)
        {
            item.RangeInFeet = srd.ThrowRange.Normal ?? 0;
            item.MaximumRangeInFeet = srd.ThrowRange.Long ?? srd.ThrowRange.Normal ?? 0;
        }

        // Parse weapon properties
        foreach (var prop in srd.Properties)
        {
            var weaponProp = MapperHelpers.ParseWeaponProperty(prop.Index);
            if (weaponProp.HasValue && !item.WeaponProperties.Contains(weaponProp.Value))
            {
                item.WeaponProperties.Add(weaponProp.Value);
            }
        }
    }

    private static void MapWeaponProperties2024(SrdEquipment2024 srd, Item item)
    {
        item.Type = ItemType.Weapon;

        // Parse weapon type
        var weaponCat = srd.WeaponCategory?.ToLowerInvariant() ?? "";
        var weaponRange = srd.WeaponRange?.ToLowerInvariant() ?? "";

        item.WeaponType = (weaponCat, weaponRange) switch
        {
            ("simple", "melee") => WeaponType.SimpleMelee,
            ("simple", "ranged") => WeaponType.SimpleRanged,
            ("martial", "melee") => WeaponType.MartialMelee,
            ("martial", "ranged") => WeaponType.MartialRanged,
            _ => WeaponType.SimpleMelee
        };

        // Parse damage
        if (srd.Damage != null)
        {
            var (diceCount, diceValue) = MapperHelpers.ParseDamageDice(srd.Damage.DamageDice);
            var damageType = MapperHelpers.ParseDamageType(srd.Damage.DamageType?.Index);

            if (damageType.HasValue)
            {
                item.Damages.Add(new AttackDamage
                {
                    DiceCount = diceCount,
                    DiceValue = diceValue,
                    Bonus = 0,
                    DamageType = damageType.Value
                });
                item.DamageTypes.Add(damageType.Value);
            }
        }

        // Parse range
        if (srd.Range?.Normal.HasValue == true)
        {
            item.RangeInFeet = srd.Range.Normal.Value;
            item.MaximumRangeInFeet = srd.Range.Long ?? srd.Range.Normal.Value;
        }

        // Parse throw range
        if (srd.ThrowRange != null)
        {
            item.RangeInFeet = srd.ThrowRange.Normal ?? 0;
            item.MaximumRangeInFeet = srd.ThrowRange.Long ?? srd.ThrowRange.Normal ?? 0;
        }

        // Parse weapon properties
        if (srd.Properties != null)
        {
            foreach (var prop in srd.Properties)
            {
                var weaponProp = MapperHelpers.ParseWeaponProperty(prop.Index);
                if (weaponProp.HasValue && !item.WeaponProperties.Contains(weaponProp.Value))
                {
                    item.WeaponProperties.Add(weaponProp.Value);
                }
            }
        }

        // Parse mastery (2024 specific)
        if (srd.Mastery != null)
        {
            item.Mastery = MapperHelpers.ParseMastery(srd.Mastery.Index);
        }
    }

    private static void MapArmorProperties(SrdEquipment srd, Item item)
    {
        var armorCategory = srd.ArmorCategory?.ToLowerInvariant() ?? "";

        item.Type = armorCategory switch
        {
            "light" => ItemType.LightArmor,
            "medium" => ItemType.MediumArmor,
            "heavy" => ItemType.HeavyArmor,
            "shield" => ItemType.Shield,
            _ => ItemType.LightArmor
        };

        if (srd.ArmorClass != null)
        {
            item.BaseArmorClass = srd.ArmorClass.Base;

            if (srd.ArmorClass.DexBonus == true)
            {
                item.ArmorClassBonusAbility = AbilityScore.Dexterity;
                item.MaximumArmorClassBonusFromAbility = srd.ArmorClass.MaxBonus ?? 99;
            }
        }

        if (srd.StrMinimum.HasValue)
        {
            item.RequiredAbilityScore = AbilityScore.Strength;
            item.RequiredAbilityScoreValue = srd.StrMinimum.Value;
        }

        item.GivesDisadvantageOnStealth = srd.StealthDisadvantage == true;
    }

    private static void MapArmorProperties2024(SrdEquipment2024 srd, Item item)
    {
        var armorCategory = srd.ArmorCategory?.ToLowerInvariant() ?? "";

        item.Type = armorCategory switch
        {
            "light" => ItemType.LightArmor,
            "medium" => ItemType.MediumArmor,
            "heavy" => ItemType.HeavyArmor,
            "shield" => ItemType.Shield,
            _ => ItemType.LightArmor
        };

        if (srd.ArmorClass != null)
        {
            item.BaseArmorClass = srd.ArmorClass.Base;

            if (srd.ArmorClass.DexBonus == true)
            {
                item.ArmorClassBonusAbility = AbilityScore.Dexterity;
                item.MaximumArmorClassBonusFromAbility = srd.ArmorClass.MaxBonus ?? 99;
            }
        }

        if (srd.StrMinimum.HasValue)
        {
            item.RequiredAbilityScore = AbilityScore.Strength;
            item.RequiredAbilityScoreValue = srd.StrMinimum.Value;
        }

        item.GivesDisadvantageOnStealth = srd.StealthDisadvantage == true;
    }
}
