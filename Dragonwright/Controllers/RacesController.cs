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
[Route("races")]
public sealed class RacesController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListRaces(
        int page = 1, int pageSize = 20, string? search = null, SourceType? source = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Races.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.ToLower().Contains(search.ToLower()));
        if (source.HasValue)
            query = query.Where(r => r.Source == source.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Race>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRace(Guid id)
    {
        var race = await dbContext.Races
            .Include(r => r.Traits).ThenInclude(t => t.Options)
            .Include(r => r.Traits).ThenInclude(t => t.Actions)
            .Include(r => r.Traits).ThenInclude(t => t.Spells)
            .Include(r => r.Traits).ThenInclude(t => t.Creatures)
            .Include(r => r.Traits).ThenInclude(t => t.Modifiers)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (race == null) return NotFound();
        return Ok(race);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRace([FromBody] Race race)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (!ValidateSourcePermission(race.Source)) return Forbid();

        race.Id = Guid.NewGuid();
        race.SourceCreatorId = userId.Value;

        // Assign new GUIDs to all nested entities to prevent DbUpdateConcurrencyException
        foreach (var trait in race.Traits)
        {
            trait.Id = Guid.NewGuid();
            foreach (var opt in trait.Options) opt.Id = Guid.NewGuid();
            foreach (var act in trait.Actions) act.Id = Guid.NewGuid();
            foreach (var spell in trait.Spells) spell.Id = Guid.NewGuid();
            foreach (var creature in trait.Creatures) creature.Id = Guid.NewGuid();
            foreach (var mod in trait.Modifiers) mod.Id = Guid.NewGuid();
        }

        dbContext.Races.Add(race);
        dbContext.ChangeTracker.TrackGraph(race, n => n.Entry.State = EntityState.Added);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRace), new { id = race.Id }, race);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRace(Guid id, [FromBody] Race updated)
    {
        var race = await dbContext.Races.FindAsync(id);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        race.Name = updated.Name;
        race.Source = updated.Source;
        race.Type = updated.Type;
        race.ImageId = updated.ImageId;

        await dbContext.SaveChangesAsync();
        return Ok(race);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRace(Guid id)
    {
        var race = await dbContext.Races.FindAsync(id);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        dbContext.Races.Remove(race);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{raceId:guid}/traits")]
    public async Task<IActionResult> ListTraits(Guid raceId)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();

        var traits = await dbContext.RaceTraits
            .Where(t => dbContext.Races.Where(r => r.Id == raceId).SelectMany(r => r.Traits).Select(tr => tr.Id).Contains(t.Id))
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();

        return Ok(traits);
    }

    [HttpGet("{raceId:guid}/traits/{id:guid}")]
    public async Task<IActionResult> GetTrait(Guid raceId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();

        var trait = await dbContext.RaceTraits
            .Include(t => t.Options)
            .Include(t => t.Actions)
            .Include(t => t.Spells)
            .Include(t => t.Creatures)
            .Include(t => t.Modifiers)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trait == null) return NotFound();
        return Ok(trait);
    }

    [HttpPost("{raceId:guid}/traits")]
    public async Task<IActionResult> CreateTrait(Guid raceId, [FromBody] RaceTrait trait)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var race = await dbContext.Races.Include(r => r.Traits).FirstOrDefaultAsync(r => r.Id == raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        trait.Id = Guid.NewGuid();
        foreach (var opt in trait.Options) opt.Id = Guid.NewGuid();
        foreach (var act in trait.Actions) act.Id = Guid.NewGuid();
        foreach (var spell in trait.Spells) spell.Id = Guid.NewGuid();
        foreach (var creature in trait.Creatures) creature.Id = Guid.NewGuid();
        foreach (var mod in trait.Modifiers) mod.Id = Guid.NewGuid();
        race.Traits.Add(trait);
        dbContext.ChangeTracker.TrackGraph(trait, n => n.Entry.State = EntityState.Added);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTrait), new { raceId, id = trait.Id }, trait);
    }

    [HttpPut("{raceId:guid}/traits/{id:guid}")]
    public async Task<IActionResult> UpdateTrait(Guid raceId, Guid id, [FromBody] RaceTrait updated)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.FindAsync(id);
        if (trait == null) return NotFound();

        trait.Name = updated.Name;
        trait.Description = updated.Description;
        trait.DisplayOrder = updated.DisplayOrder;
        trait.HideInBuilder = updated.HideInBuilder;
        trait.HideInCharacterSheet = updated.HideInCharacterSheet;
        trait.FeatureType = updated.FeatureType;
        trait.TraitToReplaceId = updated.TraitToReplaceId;
        trait.CharactersLevelWhereOptionsArePresented = updated.CharactersLevelWhereOptionsArePresented;
        trait.RequiredCharacterLevel = updated.RequiredCharacterLevel;

        await dbContext.SaveChangesAsync();
        return Ok(trait);
    }

    [HttpDelete("{raceId:guid}/traits/{id:guid}")]
    public async Task<IActionResult> DeleteTrait(Guid raceId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.FindAsync(id);
        if (trait == null) return NotFound();

        dbContext.RaceTraits.Remove(trait);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{raceId:guid}/traits/{traitId:guid}/options")]
    public async Task<IActionResult> CreateTraitOption(Guid raceId, Guid traitId, [FromBody] RaceTraitOption option)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.Include(t => t.Options).FirstOrDefaultAsync(t => t.Id == traitId);
        if (trait == null) return NotFound();

        option.Id = Guid.NewGuid();
        option.RaceTraitId = traitId;
        trait.Options.Add(option);
        await dbContext.SaveChangesAsync();
        return Created($"/races/{raceId}/traits/{traitId}/options/{option.Id}", option);
    }

    [HttpPut("{raceId:guid}/traits/{traitId:guid}/options/{id:guid}")]
    public async Task<IActionResult> UpdateTraitOption(Guid raceId, Guid traitId, Guid id, [FromBody] RaceTraitOption updated)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var option = await dbContext.RaceTraitOptions.FindAsync(id);
        if (option == null || option.RaceTraitId != traitId) return NotFound();

        option.Name = updated.Name;
        option.Description = updated.Description;
        option.RequiredOptionId = updated.RequiredOptionId;
        option.RequirementDescription = updated.RequirementDescription;
        option.RequiredCharacterLevel = updated.RequiredCharacterLevel;
        option.IsGranted = updated.IsGranted;

        await dbContext.SaveChangesAsync();
        return Ok(option);
    }

    [HttpDelete("{raceId:guid}/traits/{traitId:guid}/options/{id:guid}")]
    public async Task<IActionResult> DeleteTraitOption(Guid raceId, Guid traitId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var option = await dbContext.RaceTraitOptions.FindAsync(id);
        if (option == null || option.RaceTraitId != traitId) return NotFound();

        dbContext.RaceTraitOptions.Remove(option);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{raceId:guid}/traits/{traitId:guid}/actions")]
    public async Task<IActionResult> CreateTraitAction(Guid raceId, Guid traitId, [FromBody] RaceTraitAction action)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.Include(t => t.Actions).FirstOrDefaultAsync(t => t.Id == traitId);
        if (trait == null) return NotFound();

        action.Id = Guid.NewGuid();
        action.RaceTraitId = traitId;
        trait.Actions.Add(action);
        await dbContext.SaveChangesAsync();
        return Created($"/races/{raceId}/traits/{traitId}/actions/{action.Id}", action);
    }

    [HttpPut("{raceId:guid}/traits/{traitId:guid}/actions/{id:guid}")]
    public async Task<IActionResult> UpdateTraitAction(Guid raceId, Guid traitId, Guid id, [FromBody] RaceTraitAction updated)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var action = await dbContext.RaceTraitActions.FindAsync(id);
        if (action == null || action.RaceTraitId != traitId) return NotFound();

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

    [HttpDelete("{raceId:guid}/traits/{traitId:guid}/actions/{id:guid}")]
    public async Task<IActionResult> DeleteTraitAction(Guid raceId, Guid traitId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var action = await dbContext.RaceTraitActions.FindAsync(id);
        if (action == null || action.RaceTraitId != traitId) return NotFound();

        dbContext.RaceTraitActions.Remove(action);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{raceId:guid}/traits/{traitId:guid}/spells")]
    public async Task<IActionResult> CreateTraitSpell(Guid raceId, Guid traitId, [FromBody] RaceTraitSpell spell)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.Include(t => t.Spells).FirstOrDefaultAsync(t => t.Id == traitId);
        if (trait == null) return NotFound();

        spell.Id = Guid.NewGuid();
        spell.RaceTraitId = traitId;
        trait.Spells.Add(spell);
        await dbContext.SaveChangesAsync();
        return Created($"/races/{raceId}/traits/{traitId}/spells/{spell.Id}", spell);
    }

    [HttpPut("{raceId:guid}/traits/{traitId:guid}/spells/{id:guid}")]
    public async Task<IActionResult> UpdateTraitSpell(Guid raceId, Guid traitId, Guid id, [FromBody] RaceTraitSpell updated)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var spell = await dbContext.RaceTraitSpells.FindAsync(id);
        if (spell == null || spell.RaceTraitId != traitId) return NotFound();

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

    [HttpDelete("{raceId:guid}/traits/{traitId:guid}/spells/{id:guid}")]
    public async Task<IActionResult> DeleteTraitSpell(Guid raceId, Guid traitId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var spell = await dbContext.RaceTraitSpells.FindAsync(id);
        if (spell == null || spell.RaceTraitId != traitId) return NotFound();

        dbContext.RaceTraitSpells.Remove(spell);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{raceId:guid}/traits/{traitId:guid}/creatures")]
    public async Task<IActionResult> CreateTraitCreature(Guid raceId, Guid traitId, [FromBody] RaceTraitCreature creature)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.Include(t => t.Creatures).FirstOrDefaultAsync(t => t.Id == traitId);
        if (trait == null) return NotFound();

        creature.Id = Guid.NewGuid();
        creature.RaceTraitId = traitId;
        trait.Creatures.Add(creature);
        await dbContext.SaveChangesAsync();
        return Created($"/races/{raceId}/traits/{traitId}/creatures/{creature.Id}", creature);
    }

    [HttpPut("{raceId:guid}/traits/{traitId:guid}/creatures/{id:guid}")]
    public async Task<IActionResult> UpdateTraitCreature(Guid raceId, Guid traitId, Guid id, [FromBody] RaceTraitCreature updated)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var creature = await dbContext.RaceTraitCreatures.FindAsync(id);
        if (creature == null || creature.RaceTraitId != traitId) return NotFound();

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

    [HttpDelete("{raceId:guid}/traits/{traitId:guid}/creatures/{id:guid}")]
    public async Task<IActionResult> DeleteTraitCreature(Guid raceId, Guid traitId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var creature = await dbContext.RaceTraitCreatures.FindAsync(id);
        if (creature == null || creature.RaceTraitId != traitId) return NotFound();

        dbContext.RaceTraitCreatures.Remove(creature);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{raceId:guid}/traits/{traitId:guid}/modifiers")]
    public async Task<IActionResult> CreateTraitModifier(Guid raceId, Guid traitId, [FromBody] Modifier modifier)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var trait = await dbContext.RaceTraits.Include(t => t.Modifiers).FirstOrDefaultAsync(t => t.Id == traitId);
        if (trait == null) return NotFound();

        modifier.Id = Guid.NewGuid();
        trait.Modifiers.Add(modifier);
        dbContext.ChangeTracker.TrackGraph(modifier, n => n.Entry.State = EntityState.Added);
        await dbContext.SaveChangesAsync();
        return Created($"/races/{raceId}/traits/{traitId}/modifiers/{modifier.Id}", modifier);
    }

    [HttpPut("{raceId:guid}/traits/{traitId:guid}/modifiers/{id:guid}")]
    public async Task<IActionResult> UpdateTraitModifier(Guid raceId, Guid traitId, Guid id, [FromBody] Modifier updated)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

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

    [HttpDelete("{raceId:guid}/traits/{traitId:guid}/modifiers/{id:guid}")]
    public async Task<IActionResult> DeleteTraitModifier(Guid raceId, Guid traitId, Guid id)
    {
        var race = await dbContext.Races.FindAsync(raceId);
        if (race == null) return NotFound();
        if (!CanModifyContent(race.SourceCreatorId)) return Forbid();

        var modifier = await dbContext.Modifiers.FindAsync(id);
        if (modifier == null) return NotFound();

        dbContext.Modifiers.Remove(modifier);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
