using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Models.Srd2024;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class FeatMapper
{
    public static Feat Map(SrdFeat srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Feats[key] = id;

        var feat = new Feat
        {
            Id = id,
            Source = source,
            Name = srd.Name,
            Description = string.Join("\n\n", srd.Desc),
            FeatLevel = 0, // 2014 feats don't have levels
            IsRepeatable = false
        };

        // Map prerequisites
        if (srd.Prerequisites != null)
        {
            foreach (var prereq in srd.Prerequisites)
            {
                if (prereq.AbilityScore != null)
                {
                    feat.PrerequisiteAbilityScore = MapperHelpers.ParseAbilityScore(prereq.AbilityScore.Index);
                    feat.PrerequisiteAbilityScoreMinimum = prereq.MinimumScore;
                }
            }
        }

        return feat;
    }

    public static Feat Map(SrdFeat2024 srd, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, SourceType.One2024);
        lookup.Feats[key] = id;

        var feat = new Feat
        {
            Id = id,
            Source = SourceType.One2024,
            Name = srd.Name,
            Description = srd.Description,
            FeatLevel = GetFeatLevel(srd),
            IsRepeatable = !string.IsNullOrEmpty(srd.Repeatable)
        };

        // Map prerequisites
        if (srd.Prerequisites != null)
        {
            if (srd.Prerequisites.MinimumLevel.HasValue)
            {
                feat.PrerequisiteDescription = $"Level {srd.Prerequisites.MinimumLevel}";
            }

            if (!string.IsNullOrEmpty(srd.Prerequisites.FeatureNamed))
            {
                feat.PrerequisiteDescription = string.IsNullOrEmpty(feat.PrerequisiteDescription)
                    ? srd.Prerequisites.FeatureNamed
                    : $"{feat.PrerequisiteDescription}, {srd.Prerequisites.FeatureNamed}";

                // Check for spellcasting prerequisite
                if (srd.Prerequisites.FeatureNamed.ToLowerInvariant().Contains("spellcasting"))
                {
                    feat.PrerequisiteSpellcasting = true;
                }
            }
        }

        // Parse ability score increase from description
        ParseAbilityScoreIncrease(srd, feat);

        return feat;
    }

    private static int GetFeatLevel(SrdFeat2024 srd)
    {
        return srd.Type.ToLowerInvariant() switch
        {
            "origin" => 1,
            "general" => srd.Prerequisites?.MinimumLevel ?? 4,
            "fighting-style" => 1,
            "epic-boon" => 19,
            _ => 0
        };
    }

    private static void ParseAbilityScoreIncrease(SrdFeat2024 srd, Feat feat)
    {
        var desc = srd.Description.ToLowerInvariant();

        // Check for "Ability Score Increase" pattern in description
        if (desc.Contains("ability score increase"))
        {
            feat.AbilityScoreIncrease = 1;

            // Determine which ability scores are options
            if (desc.Contains("strength"))
                feat.AbilityScoreOptions.Add(AbilityScore.Strength);
            if (desc.Contains("dexterity"))
                feat.AbilityScoreOptions.Add(AbilityScore.Dexterity);
            if (desc.Contains("constitution"))
                feat.AbilityScoreOptions.Add(AbilityScore.Constitution);
            if (desc.Contains("intelligence"))
                feat.AbilityScoreOptions.Add(AbilityScore.Intelligence);
            if (desc.Contains("wisdom"))
                feat.AbilityScoreOptions.Add(AbilityScore.Wisdom);
            if (desc.Contains("charisma"))
                feat.AbilityScoreOptions.Add(AbilityScore.Charisma);

            // If "one ability score of your choice" or similar, add all
            if (desc.Contains("one ability score of your choice") ||
                (feat.AbilityScoreOptions.Count == 0 && desc.Contains("ability score")))
            {
                feat.AbilityScoreOptions.Clear();
                feat.AbilityScoreOptions.Add(AbilityScore.Strength);
                feat.AbilityScoreOptions.Add(AbilityScore.Dexterity);
                feat.AbilityScoreOptions.Add(AbilityScore.Constitution);
                feat.AbilityScoreOptions.Add(AbilityScore.Intelligence);
                feat.AbilityScoreOptions.Add(AbilityScore.Wisdom);
                feat.AbilityScoreOptions.Add(AbilityScore.Charisma);
            }

            // Check for +2
            if (desc.Contains("by 2"))
                feat.AbilityScoreIncrease = 2;
        }
    }
}
