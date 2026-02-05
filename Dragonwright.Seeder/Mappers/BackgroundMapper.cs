using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Models.Srd2024;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class BackgroundMapper
{
    public static Background Map(SrdBackground srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Backgrounds[key] = id;

        var background = new Background
        {
            Id = id,
            Source = source,
            Name = srd.Name
        };

        // Map skill proficiencies
        foreach (var prof in srd.StartingProficiencies)
        {
            var skill = MapperHelpers.ParseSkill(prof.Index);
            if (skill.HasValue && !background.SkillProficiencies.Contains(skill.Value))
            {
                background.SkillProficiencies.Add(skill.Value);
            }

            var tool = MapperHelpers.ParseTool(prof.Index);
            if (tool.HasValue && !background.ToolProficiencies.Contains(tool.Value))
            {
                background.ToolProficiencies.Add(tool.Value);
            }
        }

        // Language choices
        if (srd.LanguageOptions != null)
        {
            background.LanguageCount = srd.LanguageOptions.Choose;
        }

        return background;
    }

    public static Background Map(SrdBackground2024 srd, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, SourceType.One2024);
        lookup.Backgrounds[key] = id;

        var background = new Background
        {
            Id = id,
            Source = SourceType.One2024,
            Name = srd.Name
        };

        // Map ability score increases (2024 backgrounds provide ASI options)
        foreach (var ability in srd.AbilityScores)
        {
            var abilityScore = MapperHelpers.ParseAbilityScore(ability.Index);
            if (abilityScore.HasValue && !background.AbilityScoreIncreases.Contains(abilityScore.Value))
            {
                background.AbilityScoreIncreases.Add(abilityScore.Value);
            }
        }

        // Map proficiencies
        foreach (var prof in srd.Proficiencies)
        {
            var skill = MapperHelpers.ParseSkill(prof.Index);
            if (skill.HasValue && !background.SkillProficiencies.Contains(skill.Value))
            {
                background.SkillProficiencies.Add(skill.Value);
            }

            var tool = MapperHelpers.ParseTool(prof.Index);
            if (tool.HasValue && !background.ToolProficiencies.Contains(tool.Value))
            {
                background.ToolProficiencies.Add(tool.Value);
            }
        }

        return background;
    }

    /// <summary>
    /// Gets the feat index for a 2024 background.
    /// </summary>
    public static string? GetFeatIndex(SrdBackground2024 srd)
    {
        return srd.Feat?.Index;
    }

    /// <summary>
    /// Creates characteristics for a 2014 background.
    /// </summary>
    public static List<Characteristics> CreateCharacteristics(SrdBackground srd)
    {
        var characteristics = new List<Characteristics>();

        // Personality traits
        if (srd.PersonalityTraits?.From?.Options != null)
        {
            foreach (var option in srd.PersonalityTraits.From.Options)
            {
                if (!string.IsNullOrEmpty(option.String))
                {
                    characteristics.Add(new Characteristics
                    {
                        Id = Guid.NewGuid(),
                        Type = CharacteristicsType.PersonalityTrait,
                        Text = option.String
                    });
                }
            }
        }

        // Ideals
        if (srd.Ideals?.From?.Options != null)
        {
            foreach (var option in srd.Ideals.From.Options)
            {
                if (!string.IsNullOrEmpty(option.Desc))
                {
                    characteristics.Add(new Characteristics
                    {
                        Id = Guid.NewGuid(),
                        Type = CharacteristicsType.Ideal,
                        Text = option.Desc
                    });
                }
            }
        }

        // Bonds
        if (srd.Bonds?.From?.Options != null)
        {
            foreach (var option in srd.Bonds.From.Options)
            {
                if (!string.IsNullOrEmpty(option.String))
                {
                    characteristics.Add(new Characteristics
                    {
                        Id = Guid.NewGuid(),
                        Type = CharacteristicsType.Bond,
                        Text = option.String
                    });
                }
            }
        }

        // Flaws
        if (srd.Flaws?.From?.Options != null)
        {
            foreach (var option in srd.Flaws.From.Options)
            {
                if (!string.IsNullOrEmpty(option.String))
                {
                    characteristics.Add(new Characteristics
                    {
                        Id = Guid.NewGuid(),
                        Type = CharacteristicsType.Flaw,
                        Text = option.String
                    });
                }
            }
        }

        return characteristics;
    }
}
