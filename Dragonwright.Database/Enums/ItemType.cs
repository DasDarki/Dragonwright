namespace Dragonwright.Database.Enums;

public enum ItemType
{
    Unspecified,
    Weapon,
    LightArmor,
    MediumArmor,
    HeavyArmor,
    Shield,
    Consumable,
    Tool,
    Container,
    MagicItem,
    Ammunition,
    Mount,
    Vehicle,
    TradeGood,
    Treasure,
    Other
}

public static class ItemTypeExtensions
{
    public static bool IsArmor(this ItemType itemType)
    {
        return itemType is ItemType.LightArmor or ItemType.MediumArmor or ItemType.HeavyArmor;
    }
}