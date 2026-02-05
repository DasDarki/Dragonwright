using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Models.Srd2024;

namespace Dragonwright.Seeder.Services;

public class SrdFileReader
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SrdFileReader(string basePath)
    {
        _basePath = basePath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    private string Get2014Path(string fileName) => Path.Combine(_basePath, "2014", fileName);
    private string Get2024Path(string fileName) => Path.Combine(_basePath, "2024", fileName);

    private async Task<List<T>> ReadFileAsync<T>(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"Warning: File not found: {path}");
            return [];
        }

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? [];
    }

    // 2014 SRD Files
    public Task<List<SrdLanguage>> ReadLanguages2014Async() =>
        ReadFileAsync<SrdLanguage>(Get2014Path("5e-SRD-Languages.json"));

    public Task<List<SrdEquipment>> ReadEquipment2014Async() =>
        ReadFileAsync<SrdEquipment>(Get2014Path("5e-SRD-Equipment.json"));

    public Task<List<SrdSpell>> ReadSpells2014Async() =>
        ReadFileAsync<SrdSpell>(Get2014Path("5e-SRD-Spells.json"));

    public Task<List<SrdClass>> ReadClasses2014Async() =>
        ReadFileAsync<SrdClass>(Get2014Path("5e-SRD-Classes.json"));

    public Task<List<SrdSubclass>> ReadSubclasses2014Async() =>
        ReadFileAsync<SrdSubclass>(Get2014Path("5e-SRD-Subclasses.json"));

    public Task<List<SrdFeature>> ReadFeatures2014Async() =>
        ReadFileAsync<SrdFeature>(Get2014Path("5e-SRD-Features.json"));

    public Task<List<SrdRace>> ReadRaces2014Async() =>
        ReadFileAsync<SrdRace>(Get2014Path("5e-SRD-Races.json"));

    public Task<List<SrdSubrace>> ReadSubraces2014Async() =>
        ReadFileAsync<SrdSubrace>(Get2014Path("5e-SRD-Subraces.json"));

    public Task<List<SrdTrait>> ReadTraits2014Async() =>
        ReadFileAsync<SrdTrait>(Get2014Path("5e-SRD-Traits.json"));

    public Task<List<SrdBackground>> ReadBackgrounds2014Async() =>
        ReadFileAsync<SrdBackground>(Get2014Path("5e-SRD-Backgrounds.json"));

    public Task<List<SrdFeat>> ReadFeats2014Async() =>
        ReadFileAsync<SrdFeat>(Get2014Path("5e-SRD-Feats.json"));

    public Task<List<SrdMagicItem>> ReadMagicItems2014Async() =>
        ReadFileAsync<SrdMagicItem>(Get2014Path("5e-SRD-Magic-Items.json"));

    // 2024 SRD Files
    public Task<List<SrdLanguage2024>> ReadLanguages2024Async() =>
        ReadFileAsync<SrdLanguage2024>(Get2024Path("5e-SRD-Languages.json"));

    public Task<List<SrdEquipment2024>> ReadEquipment2024Async() =>
        ReadFileAsync<SrdEquipment2024>(Get2024Path("5e-SRD-Equipment.json"));

    public Task<List<SrdBackground2024>> ReadBackgrounds2024Async() =>
        ReadFileAsync<SrdBackground2024>(Get2024Path("5e-SRD-Backgrounds.json"));

    public Task<List<SrdFeat2024>> ReadFeats2024Async() =>
        ReadFileAsync<SrdFeat2024>(Get2024Path("5e-SRD-Feats.json"));
}
