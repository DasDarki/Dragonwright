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
[Route("backgrounds")]
public sealed class BackgroundsController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListBackgrounds(
        int page = 1, int pageSize = 20, string? search = null, SourceType? source = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Backgrounds.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Name.ToLower().Contains(search.ToLower()));
        if (source.HasValue)
            query = query.Where(b => b.Source == source.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Background>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBackground(Guid id)
    {
        var background = await dbContext.Backgrounds
            .Include(b => b.Characteristics)
            .Include(b => b.StartingItems).ThenInclude(si => si.Items)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (background == null) return NotFound();
        return Ok(background);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBackground([FromBody] Background background)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (!ValidateSourcePermission(background.Source)) return Forbid();

        background.Id = Guid.NewGuid();
        background.SourceCreatorId = userId.Value;
        dbContext.Backgrounds.Add(background);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBackground), new { id = background.Id }, background);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBackground(Guid id, [FromBody] Background updated)
    {
        var background = await dbContext.Backgrounds.FindAsync(id);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        background.Name = updated.Name;
        background.Source = updated.Source;
        background.LanguageCount = updated.LanguageCount;
        background.AbilityScoreIncreases = updated.AbilityScoreIncreases;
        background.LanguageRestrictions = updated.LanguageRestrictions;
        background.SkillProficiencies = updated.SkillProficiencies;
        background.ToolProficiencies = updated.ToolProficiencies;
        background.ArmorProficiencies = updated.ArmorProficiencies;
        background.WeaponProficiencies = updated.WeaponProficiencies;

        await dbContext.SaveChangesAsync();
        return Ok(background);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBackground(Guid id)
    {
        var background = await dbContext.Backgrounds.FindAsync(id);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        dbContext.Backgrounds.Remove(background);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{backgroundId:guid}/characteristics")]
    public async Task<IActionResult> CreateCharacteristic(Guid backgroundId, [FromBody] Characteristics characteristic)
    {
        var background = await dbContext.Backgrounds.Include(b => b.Characteristics).FirstOrDefaultAsync(b => b.Id == backgroundId);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        characteristic.Id = Guid.NewGuid();
        background.Characteristics.Add(characteristic);
        await dbContext.SaveChangesAsync();
        return Created($"/backgrounds/{backgroundId}/characteristics/{characteristic.Id}", characteristic);
    }

    [HttpPut("{backgroundId:guid}/characteristics/{id:guid}")]
    public async Task<IActionResult> UpdateCharacteristic(Guid backgroundId, Guid id, [FromBody] Characteristics updated)
    {
        var background = await dbContext.Backgrounds.FindAsync(backgroundId);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        var characteristic = await dbContext.Characteristics.FindAsync(id);
        if (characteristic == null) return NotFound();

        characteristic.Type = updated.Type;
        characteristic.Text = updated.Text;

        await dbContext.SaveChangesAsync();
        return Ok(characteristic);
    }

    [HttpDelete("{backgroundId:guid}/characteristics/{id:guid}")]
    public async Task<IActionResult> DeleteCharacteristic(Guid backgroundId, Guid id)
    {
        var background = await dbContext.Backgrounds.FindAsync(backgroundId);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        var characteristic = await dbContext.Characteristics.FindAsync(id);
        if (characteristic == null) return NotFound();

        dbContext.Characteristics.Remove(characteristic);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{backgroundId:guid}/starting-items")]
    public async Task<IActionResult> CreateStartingItem(Guid backgroundId, [FromBody] StartItemChoice choice)
    {
        var background = await dbContext.Backgrounds.Include(b => b.StartingItems).FirstOrDefaultAsync(b => b.Id == backgroundId);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        choice.Id = Guid.NewGuid();
        foreach (var item in choice.Items)
        {
            item.Id = Guid.NewGuid();
            item.ChoiceId = choice.Id;
        }

        background.StartingItems.Add(choice);
        await dbContext.SaveChangesAsync();
        return Created($"/backgrounds/{backgroundId}/starting-items/{choice.Id}", choice);
    }

    [HttpPut("{backgroundId:guid}/starting-items/{id:guid}")]
    public async Task<IActionResult> UpdateStartingItem(Guid backgroundId, Guid id, [FromBody] StartItemChoice updated)
    {
        var background = await dbContext.Backgrounds.FindAsync(backgroundId);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        var choice = await dbContext.StartItemChoices.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (choice == null) return NotFound();

        choice.Operator = updated.Operator;

        // Replace items
        dbContext.StartItems.RemoveRange(choice.Items);
        foreach (var item in updated.Items)
        {
            item.Id = Guid.NewGuid();
            item.ChoiceId = id;
        }
        choice.Items = updated.Items;

        await dbContext.SaveChangesAsync();
        return Ok(choice);
    }

    [HttpDelete("{backgroundId:guid}/starting-items/{id:guid}")]
    public async Task<IActionResult> DeleteStartingItem(Guid backgroundId, Guid id)
    {
        var background = await dbContext.Backgrounds.FindAsync(backgroundId);
        if (background == null) return NotFound();
        if (!CanModifyContent(background.SourceCreatorId)) return Forbid();

        var choice = await dbContext.StartItemChoices.FindAsync(id);
        if (choice == null) return NotFound();

        dbContext.StartItemChoices.Remove(choice);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
