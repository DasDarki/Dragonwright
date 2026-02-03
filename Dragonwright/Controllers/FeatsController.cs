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
[Route("feats")]
public sealed class FeatsController(AppDbContext dbContext) : ContentControllerBase
{
    // ───────────────────────── Feat CRUD ─────────────────────────

    [HttpGet]
    public async Task<IActionResult> ListFeats(
        int page = 1, int pageSize = 20, string? search = null, SourceType? source = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Feats.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.Name.ToLower().Contains(search.ToLower()));
        if (source.HasValue)
            query = query.Where(f => f.Source == source.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Feat>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFeat(Guid id)
    {
        var feat = await dbContext.Feats
            .Include(f => f.Options)
            .Include(f => f.Actions)
            .Include(f => f.Spells)
            .Include(f => f.Modifiers)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feat == null) return NotFound();
        return Ok(feat);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFeat([FromBody] Feat feat)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (!ValidateSourcePermission(feat.Source)) return Forbid();

        feat.Id = Guid.NewGuid();
        feat.SourceCreatorId = userId.Value;
        dbContext.Feats.Add(feat);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFeat), new { id = feat.Id }, feat);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFeat(Guid id, [FromBody] Feat updated)
    {
        var feat = await dbContext.Feats.FindAsync(id);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        feat.Name = updated.Name;
        feat.Source = updated.Source;
        feat.Description = updated.Description;
        feat.FeatLevel = updated.FeatLevel;
        feat.IsRepeatable = updated.IsRepeatable;
        feat.PrerequisiteDescription = updated.PrerequisiteDescription;
        feat.PrerequisiteAbilityScore = updated.PrerequisiteAbilityScore;
        feat.PrerequisiteAbilityScoreMinimum = updated.PrerequisiteAbilityScoreMinimum;
        feat.PrerequisiteSpellcasting = updated.PrerequisiteSpellcasting;
        feat.AbilityScoreOptions = updated.AbilityScoreOptions;
        feat.AbilityScoreIncrease = updated.AbilityScoreIncrease;

        await dbContext.SaveChangesAsync();
        return Ok(feat);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFeat(Guid id)
    {
        var feat = await dbContext.Feats.FindAsync(id);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        dbContext.Feats.Remove(feat);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────── FeatOption CRUD ─────────────────────────

    [HttpPost("{featId:guid}/options")]
    public async Task<IActionResult> CreateOption(Guid featId, [FromBody] FeatOption option)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        option.Id = Guid.NewGuid();
        option.FeatId = featId;
        dbContext.FeatOptions.Add(option);
        await dbContext.SaveChangesAsync();
        return Created($"/feats/{featId}/options/{option.Id}", option);
    }

    [HttpPut("{featId:guid}/options/{id:guid}")]
    public async Task<IActionResult> UpdateOption(Guid featId, Guid id, [FromBody] FeatOption updated)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var option = await dbContext.FeatOptions.FindAsync(id);
        if (option == null || option.FeatId != featId) return NotFound();

        option.Name = updated.Name;
        option.Description = updated.Description;
        option.RequiredOptionId = updated.RequiredOptionId;
        option.RequirementDescription = updated.RequirementDescription;
        option.RequiredCharacterLevel = updated.RequiredCharacterLevel;
        option.IsGranted = updated.IsGranted;

        await dbContext.SaveChangesAsync();
        return Ok(option);
    }

    [HttpDelete("{featId:guid}/options/{id:guid}")]
    public async Task<IActionResult> DeleteOption(Guid featId, Guid id)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var option = await dbContext.FeatOptions.FindAsync(id);
        if (option == null || option.FeatId != featId) return NotFound();

        dbContext.FeatOptions.Remove(option);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────── FeatAction CRUD ─────────────────────────

    [HttpPost("{featId:guid}/actions")]
    public async Task<IActionResult> CreateAction(Guid featId, [FromBody] FeatAction action)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        action.Id = Guid.NewGuid();
        action.FeatId = featId;
        dbContext.FeatActions.Add(action);
        await dbContext.SaveChangesAsync();
        return Created($"/feats/{featId}/actions/{action.Id}", action);
    }

    [HttpPut("{featId:guid}/actions/{id:guid}")]
    public async Task<IActionResult> UpdateAction(Guid featId, Guid id, [FromBody] FeatAction updated)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var action = await dbContext.FeatActions.FindAsync(id);
        if (action == null || action.FeatId != featId) return NotFound();

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

    [HttpDelete("{featId:guid}/actions/{id:guid}")]
    public async Task<IActionResult> DeleteAction(Guid featId, Guid id)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var action = await dbContext.FeatActions.FindAsync(id);
        if (action == null || action.FeatId != featId) return NotFound();

        dbContext.FeatActions.Remove(action);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────── FeatSpell CRUD ─────────────────────────

    [HttpPost("{featId:guid}/spells")]
    public async Task<IActionResult> CreateSpell(Guid featId, [FromBody] FeatSpell spell)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        spell.Id = Guid.NewGuid();
        spell.FeatId = featId;
        dbContext.FeatSpells.Add(spell);
        await dbContext.SaveChangesAsync();
        return Created($"/feats/{featId}/spells/{spell.Id}", spell);
    }

    [HttpPut("{featId:guid}/spells/{id:guid}")]
    public async Task<IActionResult> UpdateSpell(Guid featId, Guid id, [FromBody] FeatSpell updated)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var spell = await dbContext.FeatSpells.FindAsync(id);
        if (spell == null || spell.FeatId != featId) return NotFound();

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

    [HttpDelete("{featId:guid}/spells/{id:guid}")]
    public async Task<IActionResult> DeleteSpell(Guid featId, Guid id)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var spell = await dbContext.FeatSpells.FindAsync(id);
        if (spell == null || spell.FeatId != featId) return NotFound();

        dbContext.FeatSpells.Remove(spell);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────── Modifier CRUD ─────────────────────────

    [HttpPost("{featId:guid}/modifiers")]
    public async Task<IActionResult> CreateModifier(Guid featId, [FromBody] Modifier modifier)
    {
        var feat = await dbContext.Feats.Include(f => f.Modifiers).FirstOrDefaultAsync(f => f.Id == featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        modifier.Id = Guid.NewGuid();
        feat.Modifiers.Add(modifier);
        await dbContext.SaveChangesAsync();
        return Created($"/feats/{featId}/modifiers/{modifier.Id}", modifier);
    }

    [HttpPut("{featId:guid}/modifiers/{id:guid}")]
    public async Task<IActionResult> UpdateModifier(Guid featId, Guid id, [FromBody] Modifier updated)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

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

    [HttpDelete("{featId:guid}/modifiers/{id:guid}")]
    public async Task<IActionResult> DeleteModifier(Guid featId, Guid id)
    {
        var feat = await dbContext.Feats.FindAsync(featId);
        if (feat == null) return NotFound();
        if (!CanModifyContent(feat.SourceCreatorId)) return Forbid();

        var modifier = await dbContext.Modifiers.FindAsync(id);
        if (modifier == null) return NotFound();

        dbContext.Modifiers.Remove(modifier);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
