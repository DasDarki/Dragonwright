using Dragonwright.Database.Entities.Modifiers;
using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class RaceMapper
{
    public static Race Map(SrdRace srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Races[key] = id;

        var race = new Race
        {
            Id = id,
            Source = source,
            Name = srd.Name,
            Type = CreatureType.Humanoid
        };

        return race;
    }

    /// <summary>
    /// Creates the base traits for a race (speed, size, ability bonuses, languages).
    /// </summary>
    public static List<RaceTrait> CreateBaseTraits(SrdRace srd, IndexLookup lookup)
    {
        var traits = new List<RaceTrait>();
        var order = 0;

        // Size trait
        var sizeTrait = new RaceTrait
        {
            Id = Guid.NewGuid(),
            Name = "Size",
            Description = srd.SizeDescription ?? $"Your size is {srd.Size}.",
            DisplayOrder = order++,
            FeatureType = FeatureType.Granted,
            Modifiers =
            {
                new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.Size,
                    Subtype = new SizeSubtype { Size = MapperHelpers.ParseSize(srd.Size) }
                }
            }
        };
        traits.Add(sizeTrait);

        // Speed trait
        var speedTrait = new RaceTrait
        {
            Id = Guid.NewGuid(),
            Name = "Speed",
            Description = $"Your base walking speed is {srd.Speed} feet.",
            DisplayOrder = order++,
            FeatureType = FeatureType.Granted,
            Modifiers =
            {
                new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.SetBase,
                    Subtype = new SetBaseSubtype
                    {
                        Target = BonusTarget.Speed,
                        MovementType = MovementType.Walk
                    },
                    FixedValue = srd.Speed
                }
            }
        };
        traits.Add(speedTrait);

        // Ability score bonuses
        if (srd.AbilityBonuses.Count > 0)
        {
            var bonusTrait = CreateAbilityBonusTrait(srd.AbilityBonuses, order++);
            traits.Add(bonusTrait);
        }

        // Languages
        if (srd.Languages.Count > 0)
        {
            var languageTrait = CreateLanguageTrait(srd, lookup, order++);
            traits.Add(languageTrait);
        }

        return traits;
    }

    private static RaceTrait CreateAbilityBonusTrait(List<SrdAbilityBonus> bonuses, int order)
    {
        var descriptions = new List<string>();
        var modifiers = new List<Modifier>();

        foreach (var bonus in bonuses)
        {
            var ability = MapperHelpers.ParseAbilityScore(bonus.AbilityScore?.Index);
            if (ability == null) continue;

            descriptions.Add($"{ability.Value} +{bonus.Bonus}");

            modifiers.Add(new Modifier
            {
                Id = Guid.NewGuid(),
                Type = ModifierType.Bonus,
                Subtype = new BonusSubtype { Target = BonusTarget.AbilityScore },
                AbilityScore = ability,
                FixedValue = bonus.Bonus
            });
        }

        var trait = new RaceTrait
        {
            Id = Guid.NewGuid(),
            Name = "Ability Score Increase",
            Description = $"Your ability scores increase: {string.Join(", ", descriptions)}.",
            DisplayOrder = order,
            FeatureType = FeatureType.Granted
        };

        foreach (var mod in modifiers)
        {
            trait.Modifiers.Add(mod);
        }

        return trait;
    }

    private static RaceTrait CreateLanguageTrait(SrdRace srd, IndexLookup lookup, int order)
    {
        var trait = new RaceTrait
        {
            Id = Guid.NewGuid(),
            Name = "Languages",
            Description = srd.LanguageDesc ?? $"You can speak, read, and write {string.Join(" and ", srd.Languages.Select(l => l.Name))}.",
            DisplayOrder = order,
            FeatureType = FeatureType.Granted
        };

        foreach (var lang in srd.Languages)
        {
            var langKey = IndexLookup.GetSourceKey(lang.Index, SourceType.Legacy2014);
            var langId = lookup.Languages.GetValueOrDefault(langKey);

            trait.Modifiers.Add(new Modifier
            {
                Id = Guid.NewGuid(),
                Type = ModifierType.Language,
                Subtype = langId != Guid.Empty
                    ? new LanguageSubtype { LanguageId = langId }
                    : new LanguageSubtype { AnyLanguage = true }
            });
        }

        return trait;
    }

    /// <summary>
    /// Clones a race for 2024 version.
    /// </summary>
    public static Race Clone(Race source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();

        foreach (var kvp in lookup.Races.Where(k => k.Value == source.Id).ToList())
        {
            var baseName = kvp.Key.Replace("_2014", "");
            lookup.Races[$"{baseName}_2024"] = id;
        }

        return new Race
        {
            Id = id,
            Source = SourceType.One2024,
            Name = source.Name,
            Type = source.Type
        };
    }
}
