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
[Route("spells")]
public sealed class SpellsController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListSpells(
        int page = 1, int pageSize = 20, string? search = null, SourceType? source = null,
        SpellLevel? level = null, SpellSchool? school = null, Guid? classId = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Spells.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        if (source.HasValue)
            query = query.Where(s => s.Source == source.Value);
        if (level.HasValue)
            query = query.Where(s => s.Level == level.Value);
        if (school.HasValue)
            query = query.Where(s => s.School == school.Value);
        if (classId.HasValue)
            query = query.Where(s => s.Classes.Any(c => c.Id == classId.Value));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Spell>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSpell(Guid id)
    {
        var spell = await dbContext.Spells
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (spell == null) return NotFound();
        return Ok(spell);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpell([FromBody] Spell spell)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (!ValidateSourcePermission(spell.Source)) return Forbid();

        spell.Id = Guid.NewGuid();
        spell.SourceCreatorId = userId.Value;
        dbContext.Spells.Add(spell);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSpell), new { id = spell.Id }, spell);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSpell(Guid id, [FromBody] Spell updated)
    {
        var spell = await dbContext.Spells.FindAsync(id);
        if (spell == null) return NotFound();
        if (!CanModifyContent(spell.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        spell.Name = updated.Name;
        spell.Source = updated.Source;
        spell.Level = updated.Level;
        spell.Description = updated.Description;
        spell.School = updated.School;
        spell.CastingTimes = updated.CastingTimes;
        spell.HasVocalComponent = updated.HasVocalComponent;
        spell.HasSomaticComponent = updated.HasSomaticComponent;
        spell.HasMaterialComponent = updated.HasMaterialComponent;
        spell.MaterialComponents = updated.MaterialComponents;
        spell.Concentration = updated.Concentration;
        spell.Ritual = updated.Ritual;
        spell.AttackType = updated.AttackType;
        spell.Save = updated.Save;
        spell.Range = updated.Range;
        spell.AreaOfEffect = updated.AreaOfEffect;
        spell.AreaSize = updated.AreaSize;
        spell.DamageTypes = updated.DamageTypes;
        spell.Damages = updated.Damages;
        spell.Conditions = updated.Conditions;
        spell.Durations = updated.Durations;
        spell.Tags = updated.Tags;

        await dbContext.SaveChangesAsync();
        return Ok(spell);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSpell(Guid id)
    {
        var spell = await dbContext.Spells.FindAsync(id);
        if (spell == null) return NotFound();
        if (!CanModifyContent(spell.SourceCreatorId)) return Forbid();

        dbContext.Spells.Remove(spell);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
