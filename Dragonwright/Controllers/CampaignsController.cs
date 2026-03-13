using System.Security.Claims;
using System.Security.Cryptography;
using Dragonwright.Database;
using Dragonwright.Database.Entities;
using Dragonwright.Database.Enums;
using Dragonwright.Models;
using Dragonwright.Models.Campaigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Controllers;

[Authorize]
[ApiController]
[Route("campaigns")]
public sealed class CampaignsController(AppDbContext dbContext) : ControllerBase
{
    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Lists campaigns the current user is a member of or is the GM for.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListCampaigns(int page = 1, int pageSize = 20, string? search = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = dbContext.Campaigns
            .Include(c => c.GameMaster)
            .Include(c => c.Members)
            .Where(c => c.GameMasterId == userId.Value || c.Members.Any(m => m.UserId == userId.Value));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));

        var totalCount = await query.CountAsync();
        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = campaigns.Select(c => new CampaignListItem
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            IsGameMaster = c.GameMasterId == userId.Value,
            MemberCount = c.Members.Count,
            GameMaster = new CampaignUserInfo
            {
                Id = c.GameMaster.Id,
                Username = c.GameMaster.Username,
                AvatarId = c.GameMaster.AvatarId
            }
        }).ToList();

        return Ok(new PaginatedResponse<CampaignListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Gets a campaign by ID with full member details (visibility-filtered).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCampaign(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await LoadCampaignFull(id);
        if (campaign == null) return NotFound();

        if (!IsMemberOrGm(campaign, userId.Value)) return Forbid();

        return Ok(CampaignResponse.FromEntity(campaign, userId.Value));
    }

    /// <summary>
    /// Creates a new campaign. The current user becomes the GM.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            GameMasterId = userId.Value,
            InviteCode = GenerateInviteCode(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Campaigns.Add(campaign);
        await dbContext.SaveChangesAsync();

        var created = await LoadCampaignFull(campaign.Id);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id },
            CampaignResponse.FromEntity(created!, userId.Value));
    }

    /// <summary>
    /// Updates campaign name/description. GM only.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCampaign(Guid id, [FromBody] UpdateCampaignRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await dbContext.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();
        if (campaign.GameMasterId != userId.Value) return Forbid();

        campaign.Name = request.Name;
        campaign.Description = request.Description;
        await dbContext.SaveChangesAsync();

        var full = await LoadCampaignFull(id);
        return Ok(CampaignResponse.FromEntity(full!, userId.Value));
    }

    /// <summary>
    /// Deletes a campaign. GM only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCampaign(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await dbContext.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();
        if (campaign.GameMasterId != userId.Value) return Forbid();

        dbContext.Campaigns.Remove(campaign);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Regenerates the invite code. GM only.
    /// </summary>
    [HttpPost("{id:guid}/regenerate-invite")]
    public async Task<IActionResult> RegenerateInviteCode(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await dbContext.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();
        if (campaign.GameMasterId != userId.Value) return Forbid();

        campaign.InviteCode = GenerateInviteCode();
        await dbContext.SaveChangesAsync();

        return Ok(new { campaign.InviteCode });
    }

    /// <summary>
    /// Joins a campaign via invite code.
    /// </summary>
    [HttpPost("join")]
    public async Task<IActionResult> JoinCampaign([FromBody] JoinCampaignRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await dbContext.Campaigns
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.InviteCode == request.InviteCode);

        if (campaign == null) return NotFound("Invalid invite code.");

        if (campaign.GameMasterId == userId.Value)
            return BadRequest("You are the GM of this campaign.");

        if (campaign.Members.Any(m => m.UserId == userId.Value))
            return BadRequest("You are already a member of this campaign.");

        var member = new CampaignMember
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            UserId = userId.Value,
            CharacterVisibility = CharacterVisibility.CampaignPrivate,
            JoinedAt = DateTime.UtcNow
        };

        dbContext.CampaignMembers.Add(member);
        await dbContext.SaveChangesAsync();

        return Ok(new { campaignId = campaign.Id });
    }

    /// <summary>
    /// Kicks a member from the campaign. GM only.
    /// </summary>
    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> KickMember(Guid id, Guid memberId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await dbContext.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();
        if (campaign.GameMasterId != userId.Value) return Forbid();

        var member = await dbContext.CampaignMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.CampaignId == id);

        if (member == null) return NotFound();

        dbContext.CampaignMembers.Remove(member);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Leave a campaign. Players only (GM cannot leave, they must delete).
    /// </summary>
    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> LeaveCampaign(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var campaign = await dbContext.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();

        if (campaign.GameMasterId == userId.Value)
            return BadRequest("The GM cannot leave the campaign. Delete it instead.");

        var member = await dbContext.CampaignMembers
            .FirstOrDefaultAsync(m => m.CampaignId == id && m.UserId == userId.Value);

        if (member == null) return NotFound("You are not a member of this campaign.");

        dbContext.CampaignMembers.Remove(member);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Links or unlinks a character to the current user's membership.
    /// </summary>
    [HttpPut("{id:guid}/my-character")]
    public async Task<IActionResult> LinkCharacter(Guid id, [FromBody] LinkCharacterRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var member = await dbContext.CampaignMembers
            .FirstOrDefaultAsync(m => m.CampaignId == id && m.UserId == userId.Value);

        if (member == null) return NotFound("You are not a member of this campaign.");

        if (request.CharacterId.HasValue)
        {
            var character = await dbContext.Characters.FindAsync(request.CharacterId.Value);
            if (character == null) return BadRequest("Character not found.");
            if (character.UserId != userId.Value) return Forbid();
        }

        member.CharacterId = request.CharacterId;
        await dbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Sets the visibility level of the current user's character in the campaign.
    /// </summary>
    [HttpPut("{id:guid}/my-visibility")]
    public async Task<IActionResult> SetVisibility(Guid id, [FromBody] SetVisibilityRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var member = await dbContext.CampaignMembers
            .FirstOrDefaultAsync(m => m.CampaignId == id && m.UserId == userId.Value);

        if (member == null) return NotFound("You are not a member of this campaign.");

        member.CharacterVisibility = request.Visibility;
        await dbContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<Campaign?> LoadCampaignFull(Guid id)
    {
        return await dbContext.Campaigns
            .Include(c => c.GameMaster)
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Members).ThenInclude(m => m.Character)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    private static bool IsMemberOrGm(Campaign campaign, Guid userId)
    {
        return campaign.GameMasterId == userId || campaign.Members.Any(m => m.UserId == userId);
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(8);
        var code = new char[8];
        for (var i = 0; i < 8; i++)
            code[i] = chars[bytes[i] % chars.Length];
        return new string(code);
    }
}
