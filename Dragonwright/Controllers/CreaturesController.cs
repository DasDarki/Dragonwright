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
[Route("creatures")]
public sealed class CreaturesController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListCreatures(
        int page = 1, int pageSize = 20, string? search = null,
        CreatureType? type = null, float? minCr = null, float? maxCr = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Creatures.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);
        if (minCr.HasValue)
            query = query.Where(c => c.ChallengeRating >= minCr.Value);
        if (maxCr.HasValue)
            query = query.Where(c => c.ChallengeRating <= maxCr.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Creature>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCreature(Guid id)
    {
        var creature = await dbContext.Creatures.FindAsync(id);
        if (creature == null) return NotFound();
        return Ok(creature);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCreature([FromBody] Creature creature)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        creature.Id = Guid.NewGuid();
        dbContext.Creatures.Add(creature);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCreature), new { id = creature.Id }, creature);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCreature(Guid id, [FromBody] Creature updated)
    {
        var creature = await dbContext.Creatures.FindAsync(id);
        if (creature == null) return NotFound();

        var role = GetCurrentUserRole();
        if (role is not (UserRole.Team or UserRole.Admin)) return Forbid();

        creature.Name = updated.Name;
        creature.Size = updated.Size;
        creature.Type = updated.Type;
        creature.Alignment = updated.Alignment;
        creature.ArmorClass = updated.ArmorClass;
        creature.ArmorClassNotes = updated.ArmorClassNotes;
        creature.HitPoints = updated.HitPoints;
        creature.HitPointDiceCount = updated.HitPointDiceCount;
        creature.HitPointDiceValue = updated.HitPointDiceValue;
        creature.HitPointDiceBonus = updated.HitPointDiceBonus;
        creature.Speed = updated.Speed;
        creature.FlyingSpeed = updated.FlyingSpeed;
        creature.SwimmingSpeed = updated.SwimmingSpeed;
        creature.ClimbSpeed = updated.ClimbSpeed;
        creature.ChallengeRating = updated.ChallengeRating;
        creature.XP = updated.XP;
        creature.ProficiencyBonus = updated.ProficiencyBonus;
        creature.StrengthScore = updated.StrengthScore;
        creature.StrengthSavingThrowBonus = updated.StrengthSavingThrowBonus;
        creature.DexterityScore = updated.DexterityScore;
        creature.DexteritySavingThrowBonus = updated.DexteritySavingThrowBonus;
        creature.ConstitutionScore = updated.ConstitutionScore;
        creature.ConstitutionSavingThrowBonus = updated.ConstitutionSavingThrowBonus;
        creature.IntelligenceScore = updated.IntelligenceScore;
        creature.IntelligenceSavingThrowBonus = updated.IntelligenceSavingThrowBonus;
        creature.WisdomScore = updated.WisdomScore;
        creature.WisdomSavingThrowBonus = updated.WisdomSavingThrowBonus;
        creature.CharismaScore = updated.CharismaScore;
        creature.CharismaSavingThrowBonus = updated.CharismaSavingThrowBonus;
        creature.SkillModifiers = updated.SkillModifiers;
        creature.PassivePerceptionBonus = updated.PassivePerceptionBonus;
        creature.PassiveInvestigationBonus = updated.PassiveInvestigationBonus;
        creature.PassiveInsightBonus = updated.PassiveInsightBonus;
        creature.Languages = updated.Languages;
        creature.DarkvisionRange = updated.DarkvisionRange;
        creature.TruesightRange = updated.TruesightRange;
        creature.TremorsenseRange = updated.TremorsenseRange;
        creature.BlindsightRange = updated.BlindsightRange;
        creature.Traits = updated.Traits;
        creature.Actions = updated.Actions;
        creature.LegendaryActions = updated.LegendaryActions;
        creature.LairActions = updated.LairActions;
        creature.Reactions = updated.Reactions;

        await dbContext.SaveChangesAsync();
        return Ok(creature);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCreature(Guid id)
    {
        var creature = await dbContext.Creatures.FindAsync(id);
        if (creature == null) return NotFound();

        var role = GetCurrentUserRole();
        if (role is not (UserRole.Team or UserRole.Admin)) return Forbid();

        dbContext.Creatures.Remove(creature);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
