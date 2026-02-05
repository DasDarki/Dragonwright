using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class ClassMapper
{
    public static Class Map(SrdClass srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Classes[key] = id;

        var cls = new Class
        {
            Id = id,
            Source = source,
            Name = srd.Name,
            HitDie = srd.HitDie,
            BaseHitPointsAtFirstLevel = srd.HitDie,
            FixHitPointsPerLevelAfterFirst = (srd.HitDie / 2) + 1,
            HitPointsModifierAbilityScore = AbilityScore.Constitution
        };

        // Map saving throw proficiencies
        foreach (var save in srd.SavingThrows)
        {
            var ability = MapperHelpers.ParseAbilityScore(save.Index);
            if (ability.HasValue)
            {
                cls.SavingThrowProficiencies.Add(ability.Value);
            }
        }

        // Map proficiencies
        MapProficiencies(srd, cls);

        // Map skill proficiency choices
        MapSkillChoices(srd, cls);

        return cls;
    }

    private static void MapProficiencies(SrdClass srd, Class cls)
    {
        foreach (var prof in srd.Proficiencies)
        {
            var index = prof.Index.ToLowerInvariant();

            // Armor proficiencies
            if (index == "light-armor") cls.ArmorProficiencies.Add(ItemType.LightArmor);
            else if (index == "medium-armor") cls.ArmorProficiencies.Add(ItemType.MediumArmor);
            else if (index == "heavy-armor") cls.ArmorProficiencies.Add(ItemType.HeavyArmor);
            else if (index == "shields") cls.ArmorProficiencies.Add(ItemType.Shield);

            // Weapon proficiencies
            else if (index == "simple-weapons")
            {
                cls.WeaponProficiencies.Add(WeaponType.SimpleMelee);
                cls.WeaponProficiencies.Add(WeaponType.SimpleRanged);
            }
            else if (index == "martial-weapons")
            {
                cls.WeaponProficiencies.Add(WeaponType.MartialMelee);
                cls.WeaponProficiencies.Add(WeaponType.MartialRanged);
            }

            // Tool proficiencies
            var tool = MapperHelpers.ParseTool(index);
            if (tool.HasValue && !cls.ToolProficiencies.Contains(tool.Value))
            {
                cls.ToolProficiencies.Add(tool.Value);
            }
        }
    }

    private static void MapSkillChoices(SrdClass srd, Class cls)
    {
        if (srd.ProficiencyChoices == null) return;

        foreach (var choice in srd.ProficiencyChoices)
        {
            if (choice.Type?.ToLowerInvariant() != "proficiencies") continue;

            cls.SkillProficienciesCount = choice.Choose;

            if (choice.From?.Options == null) continue;

            foreach (var option in choice.From.Options)
            {
                if (option.Item == null) continue;

                var skill = MapperHelpers.ParseSkill(option.Item.Index);
                if (skill.HasValue && !cls.SkillProficienciesOptions.Contains(skill.Value))
                {
                    cls.SkillProficienciesOptions.Add(skill.Value);
                }
            }
        }
    }

    /// <summary>
    /// Clones a class for 2024 version.
    /// </summary>
    public static Class Clone(Class source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var originalKey = IndexLookup.GetSourceKey(source.Name.ToLowerInvariant(), SourceType.Legacy2014);
        var newKey = IndexLookup.GetSourceKey(source.Name.ToLowerInvariant(), SourceType.One2024);

        // Use the original's index name for lookup
        foreach (var kvp in lookup.Classes.Where(k => k.Value == source.Id).ToList())
        {
            var baseName = kvp.Key.Replace("_2014", "");
            lookup.Classes[$"{baseName}_2024"] = id;
        }

        return new Class
        {
            Id = id,
            Source = SourceType.One2024,
            Name = source.Name,
            HitDie = source.HitDie,
            BaseHitPointsAtFirstLevel = source.BaseHitPointsAtFirstLevel,
            FixHitPointsPerLevelAfterFirst = source.FixHitPointsPerLevelAfterFirst,
            HitPointsModifierAbilityScore = source.HitPointsModifierAbilityScore,
            PrimaryAbilityScores = source.PrimaryAbilityScores.ToList(),
            SavingThrowProficiencies = source.SavingThrowProficiencies.ToList(),
            SkillProficienciesCount = source.SkillProficienciesCount,
            SkillProficienciesOptions = source.SkillProficienciesOptions.ToList(),
            ToolProficiencies = source.ToolProficiencies.ToList(),
            ArmorProficiencies = source.ArmorProficiencies.ToList(),
            WeaponProficiencies = source.WeaponProficiencies.ToList()
        };
    }
}
