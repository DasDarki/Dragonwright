using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Models.Srd2024;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class LanguageMapper
{
    public static Language Map(SrdLanguage srd, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, SourceType.Legacy2014);
        lookup.Languages[key] = id;

        return new Language
        {
            Id = id,
            Name = srd.Name,
            Description = srd.Desc ?? string.Empty,
            Type = srd.Type.Equals("Exotic", StringComparison.OrdinalIgnoreCase)
                ? LanguageType.Exotic
                : LanguageType.Standard,
            Script = srd.Script,
            TypicalSpeakers = srd.TypicalSpeakers
        };
    }

    public static Language Map(SrdLanguage2024 srd, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, SourceType.One2024);
        lookup.Languages[key] = id;

        return new Language
        {
            Id = id,
            Name = srd.Name,
            Description = srd.Note ?? string.Empty,
            Type = srd.IsRare ? LanguageType.Exotic : LanguageType.Standard,
            Script = null, // 2024 SRD doesn't include script info
            TypicalSpeakers = []
        };
    }
}
