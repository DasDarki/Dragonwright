using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class SubclassMapper
{
    public static Subclass Map(SrdSubclass srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Subclasses[key] = id;

        // Get the class ID
        var classKey = IndexLookup.GetSourceKey(srd.Class?.Index ?? "", source);
        var classId = lookup.Classes.GetValueOrDefault(classKey, Guid.Empty);

        var subclass = new Subclass
        {
            Id = id,
            Source = source,
            ClassId = classId,
            Name = srd.Name,
            ShortDescription = srd.SubclassFlavor ?? string.Empty,
            Description = string.Join("\n\n", srd.Desc)
        };

        return subclass;
    }

    /// <summary>
    /// Clones a subclass for 2024 version.
    /// </summary>
    public static Subclass Clone(Subclass source, Guid newClassId, IndexLookup lookup)
    {
        var id = Guid.NewGuid();

        // Register the new subclass in lookup
        foreach (var kvp in lookup.Subclasses.Where(k => k.Value == source.Id).ToList())
        {
            var baseName = kvp.Key.Replace("_2014", "");
            lookup.Subclasses[$"{baseName}_2024"] = id;
        }

        return new Subclass
        {
            Id = id,
            Source = SourceType.One2024,
            ClassId = newClassId,
            Name = source.Name,
            ShortDescription = source.ShortDescription,
            Description = source.Description,
            CanCastSpells = source.CanCastSpells,
            SpellcastingAbility = source.SpellcastingAbility,
            KnowsAllSpells = source.KnowsAllSpells,
            SpellPrepareType = source.SpellPrepareType,
            SpellLearnType = source.SpellLearnType
        };
    }
}
