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
[Route("items")]
public sealed class ItemsController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListItems(
        int page = 1, int pageSize = 20, string? search = null, SourceType? source = null,
        ItemType? type = null, Rarity? rarity = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Items.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Name.ToLower().Contains(search.ToLower()));
        if (source.HasValue)
            query = query.Where(i => i.Source == source.Value);
        if (type.HasValue)
            query = query.Where(i => i.Type == type.Value);
        if (rarity.HasValue)
            query = query.Where(i => i.Rarity == rarity.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Item>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetItem(Guid id)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] Item item)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (!ValidateSourcePermission(item.Source)) return Forbid();

        item.Id = Guid.NewGuid();
        item.SourceCreatorId = userId.Value;
        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] Item updated)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item == null) return NotFound();
        if (!CanModifyContent(item.SourceCreatorId)) return Forbid();
        if (!ValidateSourcePermission(updated.Source)) return Forbid();

        item.Name = updated.Name;
        item.Source = updated.Source;
        item.Description = updated.Description;
        item.IsMagical = updated.IsMagical;
        item.RequiresAttunement = updated.RequiresAttunement;
        item.IsConsumable = updated.IsConsumable;
        item.Type = updated.Type;
        item.Rarity = updated.Rarity;
        item.WeightInOunces = updated.WeightInOunces;
        item.ValueInCopper = updated.ValueInCopper;
        item.WeaponType = updated.WeaponType;
        item.BaseArmorClass = updated.BaseArmorClass;
        item.ArmorClassBonus = updated.ArmorClassBonus;
        item.ArmorClassBonusAbility = updated.ArmorClassBonusAbility;
        item.MaximumArmorClassBonusFromAbility = updated.MaximumArmorClassBonusFromAbility;
        item.GivesDisadvantageOnStealth = updated.GivesDisadvantageOnStealth;
        item.DonningTimeInSeconds = updated.DonningTimeInSeconds;
        item.DoffingTimeInSeconds = updated.DoffingTimeInSeconds;
        item.RequiredAbilityScore = updated.RequiredAbilityScore;
        item.RequiredAbilityScoreValue = updated.RequiredAbilityScoreValue;
        item.WeaponProperties = updated.WeaponProperties;
        item.RangeInFeet = updated.RangeInFeet;
        item.MaximumRangeInFeet = updated.MaximumRangeInFeet;
        item.AttackBonus = updated.AttackBonus;
        item.Damages = updated.Damages;
        item.DamageBonusAbility = updated.DamageBonusAbility;
        item.DamageTypes = updated.DamageTypes;
        item.Mastery = updated.Mastery;
        item.IsBackpack = updated.IsBackpack;
        item.ToolType = updated.ToolType;

        await dbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item == null) return NotFound();
        if (!CanModifyContent(item.SourceCreatorId)) return Forbid();

        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
