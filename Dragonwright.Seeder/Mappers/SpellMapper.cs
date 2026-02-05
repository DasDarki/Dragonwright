using Dragonwright.Database.Entities.Models;
using Dragonwright.Seeder.Models.Srd2014;
using Dragonwright.Seeder.Services;

namespace Dragonwright.Seeder.Mappers;

public static class SpellMapper
{
    public static Spell Map(SrdSpell srd, SourceType source, IndexLookup lookup)
    {
        var id = Guid.NewGuid();
        var key = IndexLookup.GetSourceKey(srd.Index, source);
        lookup.Spells[key] = id;

        var spell = new Spell
        {
            Id = id,
            Source = source,
            Name = srd.Name,
            Description = BuildDescription(srd),
            Level = (SpellLevel)srd.Level,
            School = MapperHelpers.ParseSpellSchool(srd.School?.Index),
            Ritual = srd.Ritual,
            Concentration = srd.Concentration,
            Range = MapperHelpers.ParseRange(srd.Range),
            AttackType = MapperHelpers.ParseAttackType(srd.AttackType),
            HasVocalComponent = srd.Components.Contains("V"),
            HasSomaticComponent = srd.Components.Contains("S"),
            HasMaterialComponent = srd.Components.Contains("M"),
            MaterialComponents = srd.Material
        };

        // Parse casting time
        var castingTime = MapperHelpers.ParseTime(srd.CastingTime);
        if (castingTime != null)
        {
            spell.CastingTimes.Add(castingTime);
        }

        // Parse duration
        var duration = MapperHelpers.ParseTime(srd.Duration);
        if (duration != null)
        {
            spell.Durations.Add(duration);
        }

        // Parse save
        if (srd.Dc?.DcType != null)
        {
            spell.Save = MapperHelpers.ParseAbilityScore(srd.Dc.DcType.Index);
        }

        // Parse area of effect
        if (srd.AreaOfEffect != null)
        {
            spell.AreaOfEffect = MapperHelpers.ParseShape(srd.AreaOfEffect.Type);
            spell.AreaSize = srd.AreaOfEffect.Size;
        }

        // Parse damage
        if (srd.Damage != null)
        {
            var damageType = MapperHelpers.ParseDamageType(srd.Damage.DamageType?.Index);
            if (damageType.HasValue)
            {
                spell.DamageTypes.Add(damageType.Value);
            }

            // Parse damage at base level
            var baseDamage = GetBaseDamage(srd);
            if (baseDamage != null)
            {
                var (diceCount, diceValue) = MapperHelpers.ParseDamageDice(baseDamage);
                if (diceValue > 0)
                {
                    spell.Damages.Add(new AttackDamage
                    {
                        DiceCount = diceCount,
                        DiceValue = diceValue,
                        Bonus = 0,
                        DamageType = damageType ?? DamageType.Force
                    });
                }
            }
        }

        return spell;
    }

    private static string BuildDescription(SrdSpell srd)
    {
        var desc = string.Join("\n\n", srd.Desc);

        if (srd.HigherLevel != null && srd.HigherLevel.Count > 0)
        {
            desc += "\n\n**At Higher Levels.** " + string.Join("\n", srd.HigherLevel);
        }

        return desc;
    }

    private static string? GetBaseDamage(SrdSpell srd)
    {
        if (srd.Damage?.DamageAtSlotLevel != null)
        {
            // Get the base level damage
            var level = srd.Level.ToString();
            if (srd.Damage.DamageAtSlotLevel.TryGetValue(level, out var damage))
            {
                return damage;
            }
            // Try to get first entry
            return srd.Damage.DamageAtSlotLevel.Values.FirstOrDefault();
        }

        if (srd.Damage?.DamageAtCharacterLevel != null)
        {
            // Get the first (lowest level) damage
            return srd.Damage.DamageAtCharacterLevel.Values.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Gets the class indices that this spell belongs to.
    /// </summary>
    public static List<string> GetClassIndices(SrdSpell srd)
    {
        return srd.Classes.Select(c => c.Index).ToList();
    }
}
