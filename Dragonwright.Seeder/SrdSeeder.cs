using Dragonwright.Seeder.Mappers;
using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Seeder;

public class SrdSeeder
{
    private readonly AppDbContext _context;
    private readonly SrdFileReader _fileReader;
    private readonly IndexLookup _lookup;

    public SrdSeeder(AppDbContext context, SrdFileReader fileReader)
    {
        _context = context;
        _fileReader = fileReader;
        _lookup = new IndexLookup();
    }
    
    private async Task<bool> HasAnyAsync<T>() where T : class
    {
        return await _context.Set<T>().AnyAsync();
    }

    public async Task SeedAsync(SeederOptions options)
    {
        // Seed in dependency order
        if (options.ShouldSeedEntity("languages"))
        {
            await SeedLanguagesAsync(options);
        }

        if (options.ShouldSeedEntity("items"))
        {
            await SeedItemsAsync(options);
        }

        if (options.ShouldSeedEntity("spells"))
        {
            await SeedSpellsAsync(options);
        }

        if (options.ShouldSeedEntity("classes"))
        {
            await SeedClassesAsync(options);
        }

        if (options.ShouldSeedEntity("races"))
        {
            await SeedRacesAsync(options);
        }

        if (options.ShouldSeedEntity("feats"))
        {
            await SeedFeatsAsync(options);
        }

        if (options.ShouldSeedEntity("backgrounds"))
        {
            await SeedBackgroundsAsync(options);
        }

        // Link spells to classes (needs to happen after both are seeded)
        if (options.ShouldSeedEntity("spells") && options.ShouldSeedEntity("classes"))
        {
            await LinkSpellsToClassesAsync(options);
        }
    }

    private async Task CleanSrdContentAsync()
    {
        Console.WriteLine("Cleaning existing SRD content...");

        // Delete in reverse dependency order
        await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM ""Characteristics"";
            DELETE FROM ""RaceTraitOption"";
            DELETE FROM ""RaceTraitAction"";
            DELETE FROM ""RaceTraitCreature"";
            DELETE FROM ""RaceTraitSpell"";
            DELETE FROM ""ClassFeatureOption"";
            DELETE FROM ""ClassFeatureAction"";
            DELETE FROM ""ClassFeatureCreature"";
            DELETE FROM ""ClassFeatureSpell"";
            DELETE FROM ""ClassFeatureLevelScale"";
            DELETE FROM ""FeatOption"";
            DELETE FROM ""FeatAction"";
            DELETE FROM ""FeatSpell"";
            DELETE FROM ""Modifier"";
            DELETE FROM ""StartItem"";
            DELETE FROM ""StartItemChoice"";
        ");

        // Delete main entities where SourceCreatorId is null (SRD content)
        await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM ""RaceTrait"" WHERE ""Id"" IN (
                SELECT rt.""Id"" FROM ""RaceTrait"" rt
                INNER JOIN ""Race"" r ON rt.""RaceId"" = r.""Id""
                WHERE r.""SourceCreatorId"" IS NULL
            );
            DELETE FROM ""ClassFeature"" WHERE ""Id"" IN (
                SELECT cf.""Id"" FROM ""ClassFeature"" cf
                INNER JOIN ""Class"" c ON cf.""ClassId"" = c.""Id""
                WHERE c.""SourceCreatorId"" IS NULL
            );
            DELETE FROM ""Subclass"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Background"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Feat"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Spell"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Class"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Race"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Item"" WHERE ""SourceCreatorId"" IS NULL;
            DELETE FROM ""Language"";
        ");

        Console.WriteLine("Cleanup completed.");
    }

    private async Task SeedLanguagesAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding languages...");
        
        if (await HasAnyAsync<Language>())
        {
            Console.WriteLine("  Languages already exist in the database. Skipping seeding languages.");
            return;
        }

        if (options.Seed2014)
        {
            var srdLanguages = await _fileReader.ReadLanguages2014Async();
            var languages = srdLanguages.Select(s => LanguageMapper.Map(s, _lookup)).ToList();

            await _context.Set<Language>().AddRangeAsync(languages);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {languages.Count} 2014 languages");
        }

        if (options.Seed2024)
        {
            var srdLanguages = await _fileReader.ReadLanguages2024Async();
            var languages = srdLanguages.Select(s => LanguageMapper.Map(s, _lookup)).ToList();

            await _context.Set<Language>().AddRangeAsync(languages);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {languages.Count} 2024 languages");
        }
    }

    private async Task SeedItemsAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding items...");
        
        if (await HasAnyAsync<Item>())
        {
            Console.WriteLine("  Items already exist in the database. Skipping seeding items.");
            return;
        }

        if (options.Seed2014)
        {
            var srdItems = await _fileReader.ReadEquipment2014Async();
            var items = srdItems
                .Select(s => ItemMapper.Map(s, SourceType.Legacy2014, _lookup))
                .Where(i => i != null)
                .Cast<Item>()
                .ToList();

            await _context.Set<Item>().AddRangeAsync(items);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {items.Count} 2014 items");
        }

        if (options.Seed2024)
        {
            var srdItems = await _fileReader.ReadEquipment2024Async();
            var items = srdItems
                .Select(s => ItemMapper.Map(s, _lookup))
                .Where(i => i != null)
                .Cast<Item>()
                .ToList();

            await _context.Set<Item>().AddRangeAsync(items);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {items.Count} 2024 items");
        }
    }

    private async Task SeedSpellsAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding spells...");
        
        if (await HasAnyAsync<Spell>())
        {
            Console.WriteLine("  Spells already exist in the database. Skipping seeding spells.");
            return;
        }

        var srdSpells = await _fileReader.ReadSpells2014Async();

        if (options.Seed2014)
        {
            var spells = srdSpells.Select(s => SpellMapper.Map(s, SourceType.Legacy2014, _lookup)).ToList();

            await _context.Set<Spell>().AddRangeAsync(spells);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {spells.Count} 2014 spells");
        }

        // For 2024, clone 2014 spells since 2024 SRD doesn't have spells
        if (options.Seed2024)
        {
            var spells2024 = srdSpells.Select(s => SpellMapper.Map(s, SourceType.One2024, _lookup)).ToList();

            await _context.Set<Spell>().AddRangeAsync(spells2024);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {spells2024.Count} 2024 spells (duplicated from 2014)");
        }
    }

    private async Task SeedClassesAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding classes...");
        
        if (await HasAnyAsync<Class>())        {
            Console.WriteLine("  Classes already exist in the database. Skipping seeding classes.");
            return;
        }

        var srdClasses = await _fileReader.ReadClasses2014Async();
        var srdSubclasses = await _fileReader.ReadSubclasses2014Async();
        var srdFeatures = await _fileReader.ReadFeatures2014Async();

        if (options.Seed2014)
        {
            var classes = srdClasses.Select(s => ClassMapper.Map(s, SourceType.Legacy2014, _lookup)).ToList();
            await _context.Set<Class>().AddRangeAsync(classes);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {classes.Count} 2014 classes");

            // Seed subclasses
            var subclasses = srdSubclasses.Select(s => SubclassMapper.Map(s, SourceType.Legacy2014, _lookup)).ToList();
            await _context.Set<Subclass>().AddRangeAsync(subclasses);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {subclasses.Count} 2014 subclasses");

            // Seed features
            await SeedFeaturesAsync(srdFeatures, SourceType.Legacy2014, classes, subclasses);
        }

        // For 2024, clone 2014 classes/subclasses
        if (options.Seed2024)
        {
            var classes2014 = await _context.Set<Class>()
                .Where(c => c.Source == SourceType.Legacy2014 && c.SourceCreatorId == null)
                .ToListAsync();

            var classes2024 = new List<Class>();
            var oldToNewClassId = new Dictionary<Guid, Guid>();

            foreach (var cls in classes2014)
            {
                var newClass = ClassMapper.Clone(cls, _lookup);
                classes2024.Add(newClass);
                oldToNewClassId[cls.Id] = newClass.Id;
            }

            await _context.Set<Class>().AddRangeAsync(classes2024);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {classes2024.Count} 2024 classes (duplicated from 2014)");

            // Clone subclasses
            var subclasses2014 = await _context.Set<Subclass>()
                .Where(s => s.Source == SourceType.Legacy2014 && s.SourceCreatorId == null)
                .ToListAsync();

            var subclasses2024 = new List<Subclass>();
            foreach (var sub in subclasses2014)
            {
                if (oldToNewClassId.TryGetValue(sub.ClassId, out var newClassId))
                {
                    var newSub = SubclassMapper.Clone(sub, newClassId, _lookup);
                    subclasses2024.Add(newSub);
                }
            }

            await _context.Set<Subclass>().AddRangeAsync(subclasses2024);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {subclasses2024.Count} 2024 subclasses (duplicated from 2014)");

            // Clone features
            await SeedFeaturesAsync(srdFeatures, SourceType.One2024, classes2024, subclasses2024);
        }
    }

    private async Task SeedFeaturesAsync(List<SrdFeature> srdFeatures, SourceType source, List<Class> classes, List<Subclass> subclasses)
    {
        var classFeatures = new List<ClassFeature>();
        var subclassFeatures = new List<ClassFeature>();

        foreach (var srdFeature in srdFeatures)
        {
            var feature = FeatureMapper.Map(srdFeature, source, _lookup);
            var (classIndex, subclassIndex) = FeatureMapper.GetOwner(srdFeature);

            if (!string.IsNullOrEmpty(classIndex) && string.IsNullOrEmpty(subclassIndex))
            {
                // Class feature
                var classKey = IndexLookup.GetSourceKey(classIndex, source);
                if (_lookup.Classes.TryGetValue(classKey, out var classId))
                {
                    var cls = classes.FirstOrDefault(c => c.Id == classId);
                    if (cls != null)
                    {
                        cls.Features.Add(feature);
                        _context.ChangeTracker.TrackGraph(feature, e => e.Entry.State = EntityState.Added);
                        classFeatures.Add(feature);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(subclassIndex))
            {
                // Subclass feature
                var subclassKey = IndexLookup.GetSourceKey(subclassIndex, source);
                if (_lookup.Subclasses.TryGetValue(subclassKey, out var subclassId))
                {
                    var sub = subclasses.FirstOrDefault(s => s.Id == subclassId);
                    if (sub != null)
                    {
                        sub.ClassFeatures.Add(feature);
                        _context.ChangeTracker.TrackGraph(feature, e => e.Entry.State = EntityState.Added);
                        subclassFeatures.Add(feature);
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  Added {classFeatures.Count} class features and {subclassFeatures.Count} subclass features for {source}");
    }

    private async Task SeedRacesAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding races...");

        if (await HasAnyAsync<Race>())
        {
            Console.WriteLine("  Races already exist in the database. Skipping seeding");
            return;
        }

        var srdRaces = await _fileReader.ReadRaces2014Async();
        var srdSubraces = await _fileReader.ReadSubraces2014Async();
        var srdTraits = await _fileReader.ReadTraits2014Async();

        if (options.Seed2014)
        {
            var races = new List<Race>();

            foreach (var srdRace in srdRaces)
            {
                var race = RaceMapper.Map(srdRace, SourceType.Legacy2014, _lookup);

                // Add base traits
                var baseTraits = RaceMapper.CreateBaseTraits(srdRace, _lookup);
                foreach (var trait in baseTraits)
                {
                    race.Traits.Add(trait);
                }

                // Add specific traits from traits file
                var traitOrder = baseTraits.Count;
                foreach (var traitRef in srdRace.Traits)
                {
                    var srdTrait = srdTraits.FirstOrDefault(t => t.Index == traitRef.Index);
                    if (srdTrait != null)
                    {
                        var trait = TraitMapper.Map(srdTrait, traitOrder++, _lookup);
                        race.Traits.Add(trait);
                    }
                }

                races.Add(race);
            }

            await _context.Set<Race>().AddRangeAsync(races);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {races.Count} 2014 races");
        }

        // For 2024, clone 2014 races
        if (options.Seed2024)
        {
            var races2014 = await _context.Set<Race>()
                .Include(r => r.Traits)
                .ThenInclude(t => t.Modifiers)
                .Where(r => r.Source == SourceType.Legacy2014 && r.SourceCreatorId == null)
                .ToListAsync();

            var races2024 = new List<Race>();
            foreach (var race in races2014)
            {
                var newRace = RaceMapper.Clone(race, _lookup);

                // Clone traits
                foreach (var trait in race.Traits)
                {
                    var newTrait = TraitMapper.Clone(trait);
                    newRace.Traits.Add(newTrait);
                }

                races2024.Add(newRace);
            }

            await _context.Set<Race>().AddRangeAsync(races2024);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {races2024.Count} 2024 races (duplicated from 2014)");
        }
    }

    private async Task SeedFeatsAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding feats...");
        
        if (await HasAnyAsync<Feat>())
        {
            Console.WriteLine("  Feats already exist in the database. Skipping seeding feats.");
            return;
        }
        
        if (options.Seed2014)
        {
            var srdFeats = await _fileReader.ReadFeats2014Async();
            var feats = srdFeats.Select(s => FeatMapper.Map(s, SourceType.Legacy2014, _lookup)).ToList();

            await _context.Set<Feat>().AddRangeAsync(feats);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {feats.Count} 2014 feats");
        }

        if (options.Seed2024)
        {
            var srdFeats = await _fileReader.ReadFeats2024Async();
            var feats = srdFeats.Select(s => FeatMapper.Map(s, _lookup)).ToList();

            await _context.Set<Feat>().AddRangeAsync(feats);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {feats.Count} 2024 feats");
        }
    }

    private async Task SeedBackgroundsAsync(SeederOptions options)
    {
        Console.WriteLine("\nSeeding backgrounds...");
        
        if (await HasAnyAsync<Background>())
        {
            Console.WriteLine("  Backgrounds already exist in the database. Skipping seeding backgrounds.");
            return;
        }

        if (options.Seed2014)
        {
            var srdBackgrounds = await _fileReader.ReadBackgrounds2014Async();
            var backgrounds = new List<Background>();

            foreach (var srdBg in srdBackgrounds)
            {
                var background = BackgroundMapper.Map(srdBg, SourceType.Legacy2014, _lookup);

                // Add characteristics
                var characteristics = BackgroundMapper.CreateCharacteristics(srdBg);
                foreach (var c in characteristics)
                {
                    background.Characteristics.Add(c);
                }

                backgrounds.Add(background);
            }

            await _context.Set<Background>().AddRangeAsync(backgrounds);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {backgrounds.Count} 2014 backgrounds");
        }

        if (options.Seed2024)
        {
            var srdBackgrounds = await _fileReader.ReadBackgrounds2024Async();
            var backgrounds = new List<Background>();

            foreach (var srdBg in srdBackgrounds)
            {
                var background = BackgroundMapper.Map(srdBg, _lookup);

                // Link feat if present
                var featIndex = BackgroundMapper.GetFeatIndex(srdBg);
                if (!string.IsNullOrEmpty(featIndex))
                {
                    var featKey = IndexLookup.GetSourceKey(featIndex, SourceType.One2024);
                    if (_lookup.Feats.TryGetValue(featKey, out var featId))
                    {
                        var feat = await _context.Set<Feat>().FindAsync(featId);
                        if (feat != null)
                        {
                            background.GrantedFeats.Add(feat);
                        }
                    }
                }

                backgrounds.Add(background);
            }

            await _context.Set<Background>().AddRangeAsync(backgrounds);
            await _context.SaveChangesAsync();

            Console.WriteLine($"  Added {backgrounds.Count} 2024 backgrounds");
        }
    }

    private async Task LinkSpellsToClassesAsync(SeederOptions options)
    {
        Console.WriteLine("\nLinking spells to classes...");

        var srdSpells = await _fileReader.ReadSpells2014Async();
        var spellClassLinks = new Dictionary<string, List<string>>();

        foreach (var srdSpell in srdSpells)
        {
            var classIndices = SpellMapper.GetClassIndices(srdSpell);
            spellClassLinks[srdSpell.Index] = classIndices;
        }

        // Link for both source types
        var sourcesToProcess = new List<SourceType>();
        if (options.Seed2014) sourcesToProcess.Add(SourceType.Legacy2014);
        if (options.Seed2024) sourcesToProcess.Add(SourceType.One2024);

        foreach (var source in sourcesToProcess)
        {
            var spells = await _context.Set<Spell>()
                .Where(s => s.Source == source && s.SourceCreatorId == null)
                .ToListAsync();

            var classes = await _context.Set<Class>()
                .Include(c => c.SpellList)
                .Where(c => c.Source == source && c.SourceCreatorId == null)
                .ToListAsync();

            var linkCount = 0;
            foreach (var spell in spells)
            {
                // Find the SRD spell by name to get class links
                var srdSpell = srdSpells.FirstOrDefault(s =>
                    s.Name.Equals(spell.Name, StringComparison.OrdinalIgnoreCase));

                if (srdSpell == null) continue;

                foreach (var classIndex in srdSpell.Classes.Select(c => c.Index))
                {
                    var classKey = IndexLookup.GetSourceKey(classIndex, source);
                    if (_lookup.Classes.TryGetValue(classKey, out var classId))
                    {
                        var cls = classes.FirstOrDefault(c => c.Id == classId);
                        if (cls != null && !cls.SpellList.Any(s => s.Id == spell.Id))
                        {
                            cls.SpellList.Add(spell);
                            linkCount++;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"  Created {linkCount} spell-class links for {source}");
        }
    }
}
