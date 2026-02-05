using Dragonwright.Database.Entities.Modifiers;
using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class TraitMapper
{
    public static RaceTrait Map(SrdTrait srd, int displayOrder, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = srd.Index;
        lookup.Traits[key] = id;

        var trait = new RaceTrait
        {
            Id = id,
            Name = srd.Name,
            Description = string.Join("\n\n", srd.Desc),
            DisplayOrder = displayOrder,
            FeatureType = FeatureType.Granted
        };

        // Add modifiers based on trait name/description
        AddModifiers(srd, trait, lookup);

        return trait;
    }

    private static FeatureType DetermineFeatureType(SrdTrait srd)
    {
        // FeatureType is: Granted, Replacement, Additional
        return FeatureType.Granted;
    }

    private static void AddModifiers(SrdTrait srd, RaceTrait trait, IndexLookup lookup)
    {
        var name = srd.Name.ToLowerInvariant();
        var desc = string.Join(" ", srd.Desc).ToLowerInvariant();

        // Darkvision
        if (name.Contains("darkvision") || name == "superior darkvision")
        {
            var range = 60;
            if (name.Contains("superior") || desc.Contains("120"))
                range = 120;

            trait.Modifiers.Add(new Modifier
            {
                Id = Guid.NewGuid(),
                Type = ModifierType.Sense,
                Subtype = new SenseSubtype { SenseType = SenseType.Darkvision },
                FixedValue = range
            });
        }

        // Damage resistance
        if (name.Contains("resilience") && desc.Contains("resistance"))
        {
            var damageType = ExtractDamageType(desc);
            if (damageType.HasValue)
            {
                trait.Modifiers.Add(new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.Resistance,
                    Subtype = new ResistanceSubtype { DamageTypes = [damageType.Value] }
                });
            }
        }

        // Advantage on saving throws
        if (desc.Contains("advantage on saving throws against"))
        {
            if (desc.Contains("poison"))
            {
                trait.Modifiers.Add(new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.Advantage,
                    Subtype = new AdvantageSubtype
                    {
                        Target = RollTarget.SavingThrow,
                        WhenAfflictedByCondition = Condition.Poisoned
                    }
                });
            }
            if (desc.Contains("charm") || desc.Contains("charmed"))
            {
                trait.Modifiers.Add(new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.Advantage,
                    Subtype = new AdvantageSubtype
                    {
                        Target = RollTarget.SavingThrow,
                        WhenAfflictedByCondition = Condition.Charmed
                    }
                });
            }
            if (desc.Contains("frighten") || desc.Contains("frightened"))
            {
                trait.Modifiers.Add(new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.Advantage,
                    Subtype = new AdvantageSubtype
                    {
                        Target = RollTarget.SavingThrow,
                        WhenAfflictedByCondition = Condition.Frightened
                    }
                });
            }
        }

        // Skill proficiencies from proficiencies list
        foreach (var prof in srd.Proficiencies)
        {
            var skill = MapperHelpers.ParseSkill(prof.Index);
            if (skill.HasValue)
            {
                trait.Modifiers.Add(new Modifier
                {
                    Id = Guid.NewGuid(),
                    Type = ModifierType.Proficiency,
                    Subtype = new ProficiencySubtype
                    {
                        Target = ProficiencyTarget.Skill,
                        Skill = skill
                    }
                });
            }
        }

        // Weapon proficiencies
        if (name.Contains("combat training") || name.Contains("weapon training"))
        {
            foreach (var prof in srd.Proficiencies)
            {
                // These are specific weapon proficiencies - stored as text for now
            }
        }
    }

    private static DamageType? ExtractDamageType(string desc)
    {
        if (desc.Contains("poison")) return DamageType.Poison;
        if (desc.Contains("fire")) return DamageType.Fire;
        if (desc.Contains("cold")) return DamageType.Cold;
        if (desc.Contains("lightning")) return DamageType.Lighting;
        if (desc.Contains("acid")) return DamageType.Acid;
        if (desc.Contains("necrotic")) return DamageType.Necrotic;
        if (desc.Contains("radiant")) return DamageType.Radiant;
        if (desc.Contains("thunder")) return DamageType.Thunder;
        if (desc.Contains("psychic")) return DamageType.Psychic;
        if (desc.Contains("force")) return DamageType.Force;

        return null;
    }

    /// <summary>
    /// Clones a trait for 2024 version.
    /// </summary>
    public static RaceTrait Clone(RaceTrait source)
    {
        var trait = new RaceTrait
        {
            Id = Guid.NewGuid(),
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            FeatureType = source.FeatureType,
            HideInBuilder = source.HideInBuilder,
            HideInCharacterSheet = source.HideInCharacterSheet,
            RequiredCharacterLevel = source.RequiredCharacterLevel
        };

        // Clone modifiers
        foreach (var mod in source.Modifiers)
        {
            trait.Modifiers.Add(new Modifier
            {
                Id = Guid.NewGuid(),
                Type = mod.Type,
                Subtype = mod.Subtype,
                AbilityScore = mod.AbilityScore,
                DiceCount = mod.DiceCount,
                DiceValue = mod.DiceValue,
                FixedValue = mod.FixedValue,
                Details = mod.Details,
                Duration = mod.Duration,
                ApplyOnMulticlass = mod.ApplyOnMulticlass
            });
        }

        return trait;
    }
}
