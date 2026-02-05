using Dragonwright.Database;
using Dragonwright.Database.Entities;
using Dragonwright.Database.Enums;
using Dragonwright.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Controllers;

[Authorize]
[ApiController]
[Route("classes")]
public sealed class ClassesController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListClasses(
        int page = 1, int pageSize = 20, string? search = null, SourceType? source = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Classes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        if (source.HasValue)
            query = query.Where(c => c.Source == source.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Class>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetClass(Guid id)
    {
        var cls = await dbContext.Classes
            .Include(c => c.Features).ThenInclude(f => f.Options)
            .Include(c => c.Features).ThenInclude(f => f.Actions)
            .Include(c => c.Features).ThenInclude(f => f.Spells)
            .Include(c => c.Features).ThenInclude(f => f.Creatures)
            .Include(c => c.Features).ThenInclude(f => f.Modifiers)
            .Include(c => c.Features).ThenInclude(f => f.LevelScales)
            .Include(c => c.Subclasses)
            .Include(c => c.StartingItems).ThenInclude(si => si.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cls == null) return NotFound();
        return Ok(cls);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass([FromBody] Class cls)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (!ValidateSourcePermission(cls.Source)) return Forbid();

        cls.Id = Guid.NewGuid();
        cls.SourceCreatorId = userId.Value;
        dbContext.Classes.Add(cls);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetClass), new { id = cls.Id }, cls);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateClass(Guid id, [FromBody] Class updated)
    {
        var cls = await dbContext.Classes.FindAsync(id);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        cls.Name = updated.Name;
        cls.Source = updated.Source;
        cls.HitDie = updated.HitDie;
        cls.FixHitPointsPerLevelAfterFirst = updated.FixHitPointsPerLevelAfterFirst;
        cls.BaseHitPointsAtFirstLevel = updated.BaseHitPointsAtFirstLevel;
        cls.HitPointsModifierAbilityScore = updated.HitPointsModifierAbilityScore;
        cls.PrimaryAbilityScores = updated.PrimaryAbilityScores;
        cls.SavingThrowProficiencies = updated.SavingThrowProficiencies;
        cls.SkillProficienciesCount = updated.SkillProficienciesCount;
        cls.SkillProficienciesOptions = updated.SkillProficienciesOptions;
        cls.ToolProficiencies = updated.ToolProficiencies;
        cls.ArmorProficiencies = updated.ArmorProficiencies;
        cls.WeaponProficiencies = updated.WeaponProficiencies;
        cls.ImageId = updated.ImageId;

        await dbContext.SaveChangesAsync();
        return Ok(cls);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClass(Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(id);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        dbContext.Classes.Remove(cls);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{classId:guid}/features")]
    public async Task<IActionResult> ListFeatures(Guid classId)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();

        var features = await dbContext.ClassFeatures
            .Where(f => dbContext.Classes.Where(c => c.Id == classId).SelectMany(c => c.Features).Select(fe => fe.Id).Contains(f.Id))
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        return Ok(features);
    }

    [HttpGet("{classId:guid}/features/{id:guid}")]
    public async Task<IActionResult> GetFeature(Guid classId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();

        var feature = await dbContext.ClassFeatures
            .Include(f => f.Options)
            .Include(f => f.Actions)
            .Include(f => f.Spells)
            .Include(f => f.Creatures)
            .Include(f => f.Modifiers)
            .Include(f => f.LevelScales)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feature == null) return NotFound();
        return Ok(feature);
    }

    [HttpPost("{classId:guid}/features")]
    public async Task<IActionResult> CreateFeature(Guid classId, [FromBody] ClassFeature feature)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var cls = await dbContext.Classes.Include(c => c.Features).FirstOrDefaultAsync(c => c.Id == classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        feature.Id = Guid.NewGuid();
        cls.Features.Add(feature);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFeature), new { classId, id = feature.Id }, feature);
    }

    [HttpPut("{classId:guid}/features/{id:guid}")]
    public async Task<IActionResult> UpdateFeature(Guid classId, Guid id, [FromBody] ClassFeature updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.FindAsync(id);
        if (feature == null) return NotFound();

        feature.IsOptional = updated.IsOptional;
        feature.Name = updated.Name;
        feature.Description = updated.Description;
        feature.DisplayOrder = updated.DisplayOrder;
        feature.HideInBuilder = updated.HideInBuilder;
        feature.HideInCharacterSheet = updated.HideInCharacterSheet;
        feature.HasOptions = updated.HasOptions;
        feature.DisplayRequiredLevel = updated.DisplayRequiredLevel;
        feature.RequiredCharacterLevel = updated.RequiredCharacterLevel;
        feature.FeatureType = updated.FeatureType;
        feature.FeatureToReplaceId = updated.FeatureToReplaceId;
        feature.ClassLevelWhereOptionsArePresented = updated.ClassLevelWhereOptionsArePresented;

        await dbContext.SaveChangesAsync();
        return Ok(feature);
    }

    [HttpDelete("{classId:guid}/features/{id:guid}")]
    public async Task<IActionResult> DeleteFeature(Guid classId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.FindAsync(id);
        if (feature == null) return NotFound();

        dbContext.ClassFeatures.Remove(feature);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{classId:guid}/features/{featureId:guid}/options")]
    public async Task<IActionResult> CreateFeatureOption(Guid classId, Guid featureId, [FromBody] ClassFeatureOption option)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.Include(f => f.Options).FirstOrDefaultAsync(f => f.Id == featureId);
        if (feature == null) return NotFound();

        option.Id = Guid.NewGuid();
        option.ClassFeatureId = featureId;
        feature.Options.Add(option);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/features/{featureId}/options/{option.Id}", option);
    }

    [HttpPut("{classId:guid}/features/{featureId:guid}/options/{id:guid}")]
    public async Task<IActionResult> UpdateFeatureOption(Guid classId, Guid featureId, Guid id, [FromBody] ClassFeatureOption updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var option = await dbContext.ClassFeatureOptions.FindAsync(id);
        if (option == null || option.ClassFeatureId != featureId) return NotFound();

        option.Name = updated.Name;
        option.Description = updated.Description;
        option.RequiredOptionId = updated.RequiredOptionId;
        option.RequirementDescription = updated.RequirementDescription;
        option.RequiredCharacterLevel = updated.RequiredCharacterLevel;
        option.IsGranted = updated.IsGranted;

        await dbContext.SaveChangesAsync();
        return Ok(option);
    }

    [HttpDelete("{classId:guid}/features/{featureId:guid}/options/{id:guid}")]
    public async Task<IActionResult> DeleteFeatureOption(Guid classId, Guid featureId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var option = await dbContext.ClassFeatureOptions.FindAsync(id);
        if (option == null || option.ClassFeatureId != featureId) return NotFound();

        dbContext.ClassFeatureOptions.Remove(option);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{classId:guid}/features/{featureId:guid}/actions")]
    public async Task<IActionResult> CreateFeatureAction(Guid classId, Guid featureId, [FromBody] ClassFeatureAction action)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.Include(f => f.Actions).FirstOrDefaultAsync(f => f.Id == featureId);
        if (feature == null) return NotFound();

        action.Id = Guid.NewGuid();
        action.ClassFeatureId = featureId;
        feature.Actions.Add(action);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/features/{featureId}/actions/{action.Id}", action);
    }

    [HttpPut("{classId:guid}/features/{featureId:guid}/actions/{id:guid}")]
    public async Task<IActionResult> UpdateFeatureAction(Guid classId, Guid featureId, Guid id, [FromBody] ClassFeatureAction updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var action = await dbContext.ClassFeatureActions.FindAsync(id);
        if (action == null || action.ClassFeatureId != featureId) return NotFound();

        action.ActionType = updated.ActionType;
        action.Name = updated.Name;
        action.AbilityScore = updated.AbilityScore;
        action.RequiredCharacterLevel = updated.RequiredCharacterLevel;
        action.IsProficient = updated.IsProficient;
        action.AttackType = updated.AttackType;
        action.Save = updated.Save;
        action.FixedSaveDC = updated.FixedSaveDC;
        action.DiceCount = updated.DiceCount;
        action.DiceValue = updated.DiceValue;
        action.FixedValue = updated.FixedValue;
        action.EffectOnMiss = updated.EffectOnMiss;
        action.EffectOnSaveSuccess = updated.EffectOnSaveSuccess;
        action.EffectOnSaveFailure = updated.EffectOnSaveFailure;
        action.IsUnarmedWeapon = updated.IsUnarmedWeapon;
        action.IsNaturalWeapon = updated.IsNaturalWeapon;
        action.DamageType = updated.DamageType;
        action.DisplayAsAttack = updated.DisplayAsAttack;
        action.EffectByMartialArts = updated.EffectByMartialArts;
        action.Range = updated.Range;
        action.MaximumRange = updated.MaximumRange;
        action.AreaOfEffect = updated.AreaOfEffect;
        action.AreaSize = updated.AreaSize;
        action.ActivationTime = updated.ActivationTime;
        action.ResetType = updated.ResetType;
        action.Description = updated.Description;

        await dbContext.SaveChangesAsync();
        return Ok(action);
    }

    [HttpDelete("{classId:guid}/features/{featureId:guid}/actions/{id:guid}")]
    public async Task<IActionResult> DeleteFeatureAction(Guid classId, Guid featureId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var action = await dbContext.ClassFeatureActions.FindAsync(id);
        if (action == null || action.ClassFeatureId != featureId) return NotFound();

        dbContext.ClassFeatureActions.Remove(action);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{classId:guid}/features/{featureId:guid}/spells")]
    public async Task<IActionResult> CreateFeatureSpell(Guid classId, Guid featureId, [FromBody] ClassFeatureSpell spell)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.Include(f => f.Spells).FirstOrDefaultAsync(f => f.Id == featureId);
        if (feature == null) return NotFound();

        spell.Id = Guid.NewGuid();
        spell.ClassFeatureId = featureId;
        feature.Spells.Add(spell);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/features/{featureId}/spells/{spell.Id}", spell);
    }

    [HttpPut("{classId:guid}/features/{featureId:guid}/spells/{id:guid}")]
    public async Task<IActionResult> UpdateFeatureSpell(Guid classId, Guid featureId, Guid id, [FromBody] ClassFeatureSpell updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var spell = await dbContext.ClassFeatureSpells.FindAsync(id);
        if (spell == null || spell.ClassFeatureId != featureId) return NotFound();

        spell.SpellId = updated.SpellId;
        spell.ClassId = updated.ClassId;
        spell.SpellLevels = updated.SpellLevels;
        spell.SpellSchools = updated.SpellSchools;
        spell.AttackTypes = updated.AttackTypes;
        spell.LevelDivisor = updated.LevelDivisor;
        spell.OnlyRitualSpells = updated.OnlyRitualSpells;
        spell.AbilityScore = updated.AbilityScore;
        spell.NumberOfUses = updated.NumberOfUses;
        spell.NumberOfUsesStatModifierOperation = updated.NumberOfUsesStatModifierOperation;
        spell.NumberOfUsesStatModifierAbility = updated.NumberOfUsesStatModifierAbility;
        spell.NumberOfUsesProficiencyBonusIfProficient = updated.NumberOfUsesProficiencyBonusIfProficient;
        spell.NumberOfUsesProficiencyOperation = updated.NumberOfUsesProficiencyOperation;
        spell.ResetType = updated.ResetType;
        spell.CastAtLevel = updated.CastAtLevel;
        spell.CastingTime = updated.CastingTime;
        spell.ActivationTimeUnit = updated.ActivationTimeUnit;
        spell.Range = updated.Range;
        spell.AdditionalDescription = updated.AdditionalDescription;
        spell.Restrictions = updated.Restrictions;
        spell.ConsumesSpellSlot = updated.ConsumesSpellSlot;
        spell.CountsAsKnownSpell = updated.CountsAsKnownSpell;
        spell.AlwaysPrepared = updated.AlwaysPrepared;
        spell.AvailableAtCharacterLevel = updated.AvailableAtCharacterLevel;
        spell.IsInfinite = updated.IsInfinite;

        await dbContext.SaveChangesAsync();
        return Ok(spell);
    }

    [HttpDelete("{classId:guid}/features/{featureId:guid}/spells/{id:guid}")]
    public async Task<IActionResult> DeleteFeatureSpell(Guid classId, Guid featureId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var spell = await dbContext.ClassFeatureSpells.FindAsync(id);
        if (spell == null || spell.ClassFeatureId != featureId) return NotFound();

        dbContext.ClassFeatureSpells.Remove(spell);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{classId:guid}/features/{featureId:guid}/creatures")]
    public async Task<IActionResult> CreateFeatureCreature(Guid classId, Guid featureId, [FromBody] ClassFeatureCreature creature)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.Include(f => f.Creatures).FirstOrDefaultAsync(f => f.Id == featureId);
        if (feature == null) return NotFound();

        creature.Id = Guid.NewGuid();
        creature.ClassFeatureId = featureId;
        feature.Creatures.Add(creature);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/features/{featureId}/creatures/{creature.Id}", creature);
    }

    [HttpPut("{classId:guid}/features/{featureId:guid}/creatures/{id:guid}")]
    public async Task<IActionResult> UpdateFeatureCreature(Guid classId, Guid featureId, Guid id, [FromBody] ClassFeatureCreature updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var creature = await dbContext.ClassFeatureCreatures.FindAsync(id);
        if (creature == null || creature.ClassFeatureId != featureId) return NotFound();

        creature.CreatureGroup = updated.CreatureGroup;
        creature.ExistingCreatureId = updated.ExistingCreatureId;
        creature.CreatureType = updated.CreatureType;
        creature.Name = updated.Name;
        creature.MaxChallengeRating = updated.MaxChallengeRating;
        creature.ChallengeRatingLevelDivisor = updated.ChallengeRatingLevelDivisor;
        creature.RestrictedMovementTypes = updated.RestrictedMovementTypes;
        creature.CreatureSizes = updated.CreatureSizes;

        await dbContext.SaveChangesAsync();
        return Ok(creature);
    }

    [HttpDelete("{classId:guid}/features/{featureId:guid}/creatures/{id:guid}")]
    public async Task<IActionResult> DeleteFeatureCreature(Guid classId, Guid featureId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var creature = await dbContext.ClassFeatureCreatures.FindAsync(id);
        if (creature == null || creature.ClassFeatureId != featureId) return NotFound();

        dbContext.ClassFeatureCreatures.Remove(creature);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{classId:guid}/features/{featureId:guid}/level-scales")]
    public async Task<IActionResult> CreateFeatureLevelScale(Guid classId, Guid featureId, [FromBody] ClassFeatureLevelScale levelScale)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.Include(f => f.LevelScales).FirstOrDefaultAsync(f => f.Id == featureId);
        if (feature == null) return NotFound();

        levelScale.Id = Guid.NewGuid();
        levelScale.ClassFeatureId = featureId;
        feature.LevelScales.Add(levelScale);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/features/{featureId}/level-scales/{levelScale.Id}", levelScale);
    }

    [HttpPut("{classId:guid}/features/{featureId:guid}/level-scales/{id:guid}")]
    public async Task<IActionResult> UpdateFeatureLevelScale(Guid classId, Guid featureId, Guid id, [FromBody] ClassFeatureLevelScale updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var levelScale = await dbContext.ClassFeatureLevelScales.FindAsync(id);
        if (levelScale == null || levelScale.ClassFeatureId != featureId) return NotFound();

        levelScale.Description = updated.Description;
        levelScale.ClassLevel = updated.ClassLevel;
        levelScale.DiceCount = updated.DiceCount;
        levelScale.DiceValue = updated.DiceValue;
        levelScale.FixedValue = updated.FixedValue;

        await dbContext.SaveChangesAsync();
        return Ok(levelScale);
    }

    [HttpDelete("{classId:guid}/features/{featureId:guid}/level-scales/{id:guid}")]
    public async Task<IActionResult> DeleteFeatureLevelScale(Guid classId, Guid featureId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var levelScale = await dbContext.ClassFeatureLevelScales.FindAsync(id);
        if (levelScale == null || levelScale.ClassFeatureId != featureId) return NotFound();

        dbContext.ClassFeatureLevelScales.Remove(levelScale);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{classId:guid}/features/{featureId:guid}/modifiers")]
    public async Task<IActionResult> CreateFeatureModifier(Guid classId, Guid featureId, [FromBody] Modifier modifier)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.Include(f => f.Modifiers).FirstOrDefaultAsync(f => f.Id == featureId);
        if (feature == null) return NotFound();

        modifier.Id = Guid.NewGuid();
        feature.Modifiers.Add(modifier);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/features/{featureId}/modifiers/{modifier.Id}", modifier);
    }

    [HttpPut("{classId:guid}/features/{featureId:guid}/modifiers/{id:guid}")]
    public async Task<IActionResult> UpdateFeatureModifier(Guid classId, Guid featureId, Guid id, [FromBody] Modifier updated)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var modifier = await dbContext.Modifiers.FindAsync(id);
        if (modifier == null) return NotFound();

        modifier.Type = updated.Type;
        modifier.Subtype = updated.Subtype;
        modifier.AbilityScore = updated.AbilityScore;
        modifier.DiceCount = updated.DiceCount;
        modifier.DiceValue = updated.DiceValue;
        modifier.FixedValue = updated.FixedValue;
        modifier.Details = updated.Details;
        modifier.Duration = updated.Duration;

        await dbContext.SaveChangesAsync();
        return Ok(modifier);
    }

    [HttpDelete("{classId:guid}/features/{featureId:guid}/modifiers/{id:guid}")]
    public async Task<IActionResult> DeleteFeatureModifier(Guid classId, Guid featureId, Guid id)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!CanModifyContent(cls.SourceCreatorId)) return Forbid();

        var modifier = await dbContext.Modifiers.FindAsync(id);
        if (modifier == null) return NotFound();

        dbContext.Modifiers.Remove(modifier);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{classId:guid}/subclasses")]
    public async Task<IActionResult> ListSubclasses(Guid classId)
    {
        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();

        var subclasses = await dbContext.Subclasses
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(subclasses);
    }

    [HttpGet("{classId:guid}/subclasses/{id:guid}")]
    public async Task<IActionResult> GetSubclass(Guid classId, Guid id)
    {
        var subclass = await dbContext.Subclasses
            .Include(s => s.ClassFeatures).ThenInclude(f => f.Options)
            .Include(s => s.ClassFeatures).ThenInclude(f => f.Actions)
            .Include(s => s.ClassFeatures).ThenInclude(f => f.Spells)
            .Include(s => s.ClassFeatures).ThenInclude(f => f.Creatures)
            .Include(s => s.ClassFeatures).ThenInclude(f => f.Modifiers)
            .Include(s => s.ClassFeatures).ThenInclude(f => f.LevelScales)
            .FirstOrDefaultAsync(s => s.Id == id && s.ClassId == classId);

        if (subclass == null) return NotFound();
        return Ok(subclass);
    }

    [HttpPost("{classId:guid}/subclasses")]
    public async Task<IActionResult> CreateSubclass(Guid classId, [FromBody] Subclass subclass)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var cls = await dbContext.Classes.FindAsync(classId);
        if (cls == null) return NotFound();
        if (!ValidateSourcePermission(subclass.Source)) return Forbid();

        subclass.Id = Guid.NewGuid();
        subclass.ClassId = classId;
        subclass.SourceCreatorId = userId.Value;
        dbContext.Subclasses.Add(subclass);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSubclass), new { classId, id = subclass.Id }, subclass);
    }

    [HttpPut("{classId:guid}/subclasses/{id:guid}")]
    public async Task<IActionResult> UpdateSubclass(Guid classId, Guid id, [FromBody] Subclass updated)
    {
        var subclass = await dbContext.Subclasses.FirstOrDefaultAsync(s => s.Id == id && s.ClassId == classId);
        if (subclass == null) return NotFound();
        if (!CanModifyContent(subclass.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        subclass.Name = updated.Name;
        subclass.Source = updated.Source;
        subclass.ShortDescription = updated.ShortDescription;
        subclass.Description = updated.Description;
        subclass.CanCastSpells = updated.CanCastSpells;
        subclass.SpellcastingAbility = updated.SpellcastingAbility;
        subclass.KnowsAllSpells = updated.KnowsAllSpells;
        subclass.SpellPrepareType = updated.SpellPrepareType;
        subclass.SpellLearnType = updated.SpellLearnType;
        subclass.ImageId = updated.ImageId;

        await dbContext.SaveChangesAsync();
        return Ok(subclass);
    }

    [HttpDelete("{classId:guid}/subclasses/{id:guid}")]
    public async Task<IActionResult> DeleteSubclass(Guid classId, Guid id)
    {
        var subclass = await dbContext.Subclasses.FirstOrDefaultAsync(s => s.Id == id && s.ClassId == classId);
        if (subclass == null) return NotFound();
        if (!CanModifyContent(subclass.SourceCreatorId)) return Forbid();

        dbContext.Subclasses.Remove(subclass);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{classId:guid}/subclasses/{subclassId:guid}/features")]
    public async Task<IActionResult> ListSubclassFeatures(Guid classId, Guid subclassId)
    {
        var subclass = await dbContext.Subclasses.FirstOrDefaultAsync(s => s.Id == subclassId && s.ClassId == classId);
        if (subclass == null) return NotFound();

        var features = await dbContext.Subclasses
            .Where(s => s.Id == subclassId)
            .SelectMany(s => s.ClassFeatures)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        return Ok(features);
    }

    [HttpPost("{classId:guid}/subclasses/{subclassId:guid}/features")]
    public async Task<IActionResult> CreateSubclassFeature(Guid classId, Guid subclassId, [FromBody] ClassFeature feature)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var subclass = await dbContext.Subclasses
            .Include(s => s.ClassFeatures)
            .FirstOrDefaultAsync(s => s.Id == subclassId && s.ClassId == classId);
        if (subclass == null) return NotFound();
        if (!CanModifyContent(subclass.SourceCreatorId)) return Forbid();

        feature.Id = Guid.NewGuid();
        subclass.ClassFeatures.Add(feature);
        await dbContext.SaveChangesAsync();
        return Created($"/classes/{classId}/subclasses/{subclassId}/features/{feature.Id}", feature);
    }

    [HttpPut("{classId:guid}/subclasses/{subclassId:guid}/features/{id:guid}")]
    public async Task<IActionResult> UpdateSubclassFeature(Guid classId, Guid subclassId, Guid id, [FromBody] ClassFeature updated)
    {
        var subclass = await dbContext.Subclasses.FirstOrDefaultAsync(s => s.Id == subclassId && s.ClassId == classId);
        if (subclass == null) return NotFound();
        if (!CanModifyContent(subclass.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.FindAsync(id);
        if (feature == null) return NotFound();

        feature.IsOptional = updated.IsOptional;
        feature.Name = updated.Name;
        feature.Description = updated.Description;
        feature.DisplayOrder = updated.DisplayOrder;
        feature.HideInBuilder = updated.HideInBuilder;
        feature.HideInCharacterSheet = updated.HideInCharacterSheet;
        feature.HasOptions = updated.HasOptions;
        feature.DisplayRequiredLevel = updated.DisplayRequiredLevel;
        feature.RequiredCharacterLevel = updated.RequiredCharacterLevel;
        feature.FeatureType = updated.FeatureType;
        feature.FeatureToReplaceId = updated.FeatureToReplaceId;
        feature.ClassLevelWhereOptionsArePresented = updated.ClassLevelWhereOptionsArePresented;

        await dbContext.SaveChangesAsync();
        return Ok(feature);
    }

    [HttpDelete("{classId:guid}/subclasses/{subclassId:guid}/features/{id:guid}")]
    public async Task<IActionResult> DeleteSubclassFeature(Guid classId, Guid subclassId, Guid id)
    {
        var subclass = await dbContext.Subclasses.FirstOrDefaultAsync(s => s.Id == subclassId && s.ClassId == classId);
        if (subclass == null) return NotFound();
        if (!CanModifyContent(subclass.SourceCreatorId)) return Forbid();

        var feature = await dbContext.ClassFeatures.FindAsync(id);
        if (feature == null) return NotFound();

        dbContext.ClassFeatures.Remove(feature);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
