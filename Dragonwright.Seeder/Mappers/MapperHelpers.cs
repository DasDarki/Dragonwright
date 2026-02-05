using Dragonwright.Database.Entities.Models;

namespace Dragonwright.Seeder.Mappers;

public static class MapperHelpers
{
    public static AbilityScore? ParseAbilityScore(string? index)
    {
        if (string.IsNullOrEmpty(index)) return null;

        return index.ToLowerInvariant() switch
        {
            "str" => AbilityScore.Strength,
            "dex" => AbilityScore.Dexterity,
            "con" => AbilityScore.Constitution,
            "int" => AbilityScore.Intelligence,
            "wis" => AbilityScore.Wisdom,
            "cha" => AbilityScore.Charisma,
            _ => null
        };
    }

    public static Skill? ParseSkill(string? index)
    {
        if (string.IsNullOrEmpty(index)) return null;

        var normalized = index.ToLowerInvariant()
            .Replace("skill-", "")
            .Replace("-", "");

        return normalized switch
        {
            "acrobatics" => Skill.Acrobatics,
            "animalhandling" => Skill.AnimalHandling,
            "arcana" => Skill.Arcana,
            "athletics" => Skill.Athletics,
            "deception" => Skill.Deception,
            "history" => Skill.History,
            "insight" => Skill.Insight,
            "intimidation" => Skill.Intimidation,
            "investigation" => Skill.Investigation,
            "medicine" => Skill.Medicine,
            "nature" => Skill.Nature,
            "perception" => Skill.Perception,
            "performance" => Skill.Performance,
            "persuasion" => Skill.Persuasion,
            "religion" => Skill.Religion,
            "sleightofhand" => Skill.SleightOfHand,
            "stealth" => Skill.Stealth,
            "survival" => Skill.Survival,
            _ => null
        };
    }

    public static Tool? ParseTool(string? index)
    {
        if (string.IsNullOrEmpty(index)) return null;

        var normalized = index.ToLowerInvariant()
            .Replace("tool-", "")
            .Replace("-", "")
            .Replace("'", "")
            .Replace(" ", "");

        return normalized switch
        {
            "alchemistssupplies" or "alchemistsupplies" => Tool.AlchemistSupplies,
            "brewerssupplies" or "brewersupplies" => Tool.BrewerSupplies,
            "calligrapherssupplies" or "calligraphersupplies" => Tool.CalligrapherSupplies,
            "carpenterstools" or "carpentertools" => Tool.CarpenterTools,
            "cartographerstools" or "cartographertools" => Tool.CartographerTools,
            "cobblerstools" or "cobblertools" => Tool.CobblerTools,
            "cooksutensils" or "cookutensils" => Tool.CookUtensils,
            "glassblowerstools" or "glassblowertools" => Tool.GlassblowerTools,
            "jewelerstools" or "jewelertools" => Tool.JewelerTools,
            "leatherworkerstools" or "leatherworkertools" => Tool.LeatherworkerTools,
            "masonstools" or "masontools" => Tool.MasonTools,
            "painterstools" or "paintertools" or "paintersupplies" => Tool.PainterSupplies,
            "potterstools" or "pottertools" => Tool.PotterTools,
            "smithstools" or "smithtools" => Tool.SmithTools,
            "tinkerstools" or "tinkertools" => Tool.TinkerTools,
            "weaverstools" or "weavertools" => Tool.WeaverTools,
            "woodcarverstools" or "woodcarvertools" => Tool.WoodcarverTools,
            "disguisekit" => Tool.DisguiseKit,
            "forgerykit" => Tool.ForgeryKit,
            "dice" or "diceset" or "dragonchess" or "dragonchessset" or
            "playingcards" or "playingcardset" or "threedragonante" or "threedragonanteset" => Tool.GamingSet,
            "herbalismkit" => Tool.HerbalismKit,
            "bagpipes" or "drum" or "dulcimer" or "flute" or "lute" or
            "lyre" or "horn" or "panflute" or "shawm" or "viol" => Tool.MusicalInstrument,
            "navigatorstools" or "navigatortools" => Tool.NavigatorTools,
            "poisonerskit" => Tool.PoisonerKit,
            "thievestools" => Tool.ThievesTools,
            _ => null
        };
    }

    public static DamageType? ParseDamageType(string? index)
    {
        if (string.IsNullOrEmpty(index)) return null;

        return index.ToLowerInvariant() switch
        {
            "bludgeoning" => DamageType.Bludgeoning,
            "piercing" => DamageType.Piercing,
            "slashing" => DamageType.Slashing,
            "lightning" => DamageType.Lighting,
            "thunder" => DamageType.Thunder,
            "poison" => DamageType.Poison,
            "cold" => DamageType.Cold,
            "radiant" => DamageType.Radiant,
            "fire" => DamageType.Fire,
            "necrotic" => DamageType.Necrotic,
            "acid" => DamageType.Acid,
            "psychic" => DamageType.Psychic,
            "force" => DamageType.Force,
            _ => null
        };
    }

    public static SpellSchool ParseSpellSchool(string? index)
    {
        if (string.IsNullOrEmpty(index)) return SpellSchool.Unspecified;

        return index.ToLowerInvariant() switch
        {
            "abjuration" => SpellSchool.Abjuration,
            "conjuration" => SpellSchool.Conjuration,
            "divination" => SpellSchool.Divination,
            "enchantment" => SpellSchool.Enchantment,
            "evocation" => SpellSchool.Evocation,
            "illusion" => SpellSchool.Illusion,
            "necromancy" => SpellSchool.Necromancy,
            "transmutation" => SpellSchool.Transmutation,
            _ => SpellSchool.Unspecified
        };
    }

    public static WeaponProperty? ParseWeaponProperty(string? index)
    {
        if (string.IsNullOrEmpty(index)) return null;

        return index.ToLowerInvariant() switch
        {
            "ammunition" => WeaponProperty.Ammunition,
            "finesse" => WeaponProperty.Finesse,
            "heavy" => WeaponProperty.Heavy,
            "light" => WeaponProperty.Light,
            "loading" => WeaponProperty.Loading,
            "range" => WeaponProperty.Range,
            "reach" => WeaponProperty.Reach,
            "special" => WeaponProperty.Special,
            "thrown" => WeaponProperty.Thrown,
            "two-handed" => WeaponProperty.TwoHanded,
            "versatile" => WeaponProperty.Versatile,
            _ => null
        };
    }

    public static Mastery? ParseMastery(string? index)
    {
        if (string.IsNullOrEmpty(index)) return null;

        return index.ToLowerInvariant() switch
        {
            "cleave" => Mastery.Cleave,
            "graze" => Mastery.Graze,
            "nick" => Mastery.Nick,
            "push" => Mastery.Push,
            "sap" => Mastery.Sap,
            "slow" => Mastery.Slow,
            "topple" => Mastery.Topple,
            "vex" => Mastery.Vex,
            _ => null
        };
    }

    public static Size ParseSize(string? size)
    {
        if (string.IsNullOrEmpty(size)) return Size.Medium;

        return size.ToLowerInvariant() switch
        {
            "tiny" => Size.Tiny,
            "small" => Size.Small,
            "medium" => Size.Medium,
            "large" => Size.Large,
            "huge" => Size.Huge,
            "gargantuan" => Size.Gargantuan,
            _ => Size.Medium
        };
    }

    public static Shape? ParseShape(string? type)
    {
        if (string.IsNullOrEmpty(type)) return null;

        return type.ToLowerInvariant() switch
        {
            "cone" => Shape.Cone,
            "cube" => Shape.Cube,
            "cylinder" => Shape.Cylinder,
            "line" => Shape.Line,
            "sphere" => Shape.Sphere,
            _ => null
        };
    }

    public static AttackType ParseAttackType(string? attackType)
    {
        if (string.IsNullOrEmpty(attackType)) return AttackType.None;

        return attackType.ToLowerInvariant() switch
        {
            "melee" => AttackType.Melee,
            "ranged" => AttackType.Ranged,
            _ => AttackType.None
        };
    }

    public static (int diceCount, int diceValue) ParseDamageDice(string? damageDice)
    {
        if (string.IsNullOrEmpty(damageDice)) return (0, 0);

        // Parse formats like "1d4", "2d6", "1d8+2", etc.
        var parts = damageDice.ToLowerInvariant().Split('d');
        if (parts.Length != 2) return (0, 0);

        if (!int.TryParse(parts[0], out var diceCount)) diceCount = 1;

        // Handle modifiers like "6+2"
        var valuePart = parts[1].Split('+')[0].Split('-')[0];
        if (!int.TryParse(valuePart, out var diceValue)) diceValue = 0;

        return (diceCount, diceValue);
    }

    public static Time? ParseTime(string? timeString)
    {
        if (string.IsNullOrEmpty(timeString)) return null;

        var lower = timeString.ToLowerInvariant().Trim();

        // Handle special cases
        if (lower == "instantaneous") return new Time { Value = 0, Unit = TimeUnit.Instantaneous };
        if (lower == "until dispelled") return new Time { Value = -1, Unit = TimeUnit.Special };
        if (lower.Contains("until dispelled")) return new Time { Value = -1, Unit = TimeUnit.Special };

        // Parse patterns like "1 action", "1 bonus action", "1 minute", "8 hours", etc.
        var parts = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return null;

        if (!int.TryParse(parts[0], out var value)) value = 1;

        var unitStr = string.Join(" ", parts.Skip(1));
        var unit = unitStr switch
        {
            "action" => TimeUnit.Action,
            "bonus action" => TimeUnit.BonusAction,
            "reaction" => TimeUnit.Reaction,
            "round" or "rounds" => TimeUnit.Round,
            "minute" or "minutes" => TimeUnit.Minute,
            "hour" or "hours" => TimeUnit.Hour,
            "day" or "days" or "week" or "weeks" => TimeUnit.Day,
            _ => TimeUnit.Special
        };

        return new Time { Value = value, Unit = unit };
    }

    public static int ParseRange(string rangeString)
    {
        if (string.IsNullOrEmpty(rangeString)) return 0;

        var lower = rangeString.ToLowerInvariant().Trim();

        if (lower == "self") return Spell.SpellRangeSelf;
        if (lower == "touch") return Spell.SpellRangeTouch;
        if (lower == "unlimited" || lower == "sight" || lower == "special") return Spell.SpellRangeInfinite;

        // Parse patterns like "90 feet", "30 feet", "1 mile"
        var parts = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return 0;

        if (!int.TryParse(parts[0], out var value)) return 0;

        // Convert miles to feet if needed
        if (parts.Length > 1 && (parts[1] == "mile" || parts[1] == "miles"))
        {
            value *= 5280;
        }

        return value;
    }

    public static int CostToCopper(int quantity, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "cp" => quantity,
            "sp" => quantity * 10,
            "ep" => quantity * 50,
            "gp" => quantity * 100,
            "pp" => quantity * 1000,
            _ => quantity
        };
    }

    public static int WeightToOunces(double weight)
    {
        // Weight in SRD is in pounds, convert to ounces
        return (int)(weight * 16);
    }
}
