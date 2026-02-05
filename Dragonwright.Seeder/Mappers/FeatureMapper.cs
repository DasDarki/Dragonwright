using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class FeatureMapper
{
    public static ClassFeature Map(SrdFeature srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Features[key] = id;

        var feature = new ClassFeature
        {
            Id = id,
            Name = srd.Name,
            Description = string.Join("\n\n", srd.Desc),
            RequiredCharacterLevel = srd.Level,
            DisplayOrder = srd.Level * 10,
            FeatureType = DetermineFeatureType(srd),
            DisplayRequiredLevel = true
        };

        return feature;
    }

    private static FeatureType DetermineFeatureType(SrdFeature srd)
    {
        // FeatureType is: Granted, Replacement, Additional
        // Most SRD features are Granted
        return FeatureType.Granted;
    }

    /// <summary>
    /// Determines if this feature belongs to a class or subclass.
    /// </summary>
    public static (string? classIndex, string? subclassIndex) GetOwner(SrdFeature srd)
    {
        return (srd.Class?.Index, srd.Subclass?.Index);
    }

    /// <summary>
    /// Clones a feature for 2024 version.
    /// </summary>
    public static ClassFeature Clone(ClassFeature source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();

        foreach (var kvp in lookup.Features.Where(k => k.Value == source.Id).ToList())
        {
            var baseName = kvp.Key.Replace("_2014", "");
            lookup.Features[$"{baseName}_2024"] = id;
        }

        return new ClassFeature
        {
            Id = id,
            Name = source.Name,
            Description = source.Description,
            RequiredCharacterLevel = source.RequiredCharacterLevel,
            DisplayOrder = source.DisplayOrder,
            FeatureType = source.FeatureType,
            DisplayRequiredLevel = source.DisplayRequiredLevel,
            IsOptional = source.IsOptional,
            HideInBuilder = source.HideInBuilder,
            HideInCharacterSheet = source.HideInCharacterSheet,
            HasOptions = source.HasOptions
        };
    }
}
