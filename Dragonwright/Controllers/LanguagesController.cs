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
[Route("languages")]
public sealed class LanguagesController(AppDbContext dbContext) : ContentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListLanguages(
        int page = 1, int pageSize = 20, string? search = null)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = dbContext.Languages.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(l => l.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Language>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLanguage(Guid id)
    {
        var language = await dbContext.Languages.FindAsync(id);
        if (language == null) return NotFound();
        return Ok(language);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLanguage([FromBody] Language language)
    {
        var role = GetCurrentUserRole();
        if (role is not (UserRole.Team or UserRole.Admin)) return Forbid();

        language.Id = Guid.NewGuid();
        dbContext.Languages.Add(language);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLanguage), new { id = language.Id }, language);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] Language updated)
    {
        var role = GetCurrentUserRole();
        if (role is not (UserRole.Team or UserRole.Admin)) return Forbid();

        var language = await dbContext.Languages.FindAsync(id);
        if (language == null) return NotFound();

        language.Name = updated.Name;
        language.Description = updated.Description;

        await dbContext.SaveChangesAsync();
        return Ok(language);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLanguage(Guid id)
    {
        var role = GetCurrentUserRole();
        if (role is not (UserRole.Team or UserRole.Admin)) return Forbid();

        var language = await dbContext.Languages.FindAsync(id);
        if (language == null) return NotFound();

        dbContext.Languages.Remove(language);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
