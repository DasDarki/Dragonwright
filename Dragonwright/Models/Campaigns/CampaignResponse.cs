using Dragonwright.Database.Entities;

namespace Dragonwright.Models.Campaigns;

public sealed class CampaignResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string InviteCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public CampaignUserInfo GameMaster { get; set; } = null!;
    public bool IsGameMaster { get; set; }
    public IReadOnlyList<CampaignMemberResponse> Members { get; set; } = [];

    public static CampaignResponse FromEntity(Campaign campaign, Guid currentUserId)
    {
        return new CampaignResponse
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            InviteCode = campaign.InviteCode,
            CreatedAt = campaign.CreatedAt,
            IsGameMaster = campaign.GameMasterId == currentUserId,
            GameMaster = new CampaignUserInfo
            {
                Id = campaign.GameMaster.Id,
                Username = campaign.GameMaster.Username,
                AvatarId = campaign.GameMaster.AvatarId
            },
            Members = campaign.Members.Select(m => CampaignMemberResponse.FromEntity(m, currentUserId, campaign.GameMasterId)).ToList()
        };
    }
}

public sealed class CampaignListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public CampaignUserInfo GameMaster { get; set; } = null!;
    public bool IsGameMaster { get; set; }
    public int MemberCount { get; set; }
}

public sealed class CampaignUserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public Guid? AvatarId { get; set; }
}

public sealed class CampaignMemberResponse
{
    public Guid Id { get; set; }
    public CampaignUserInfo User { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public string CharacterVisibility { get; set; } = null!;

    /// <summary>
    /// Character info — may be null (no character linked) or redacted depending on visibility.
    /// </summary>
    public CampaignCharacterInfo? Character { get; set; }

    public static CampaignMemberResponse FromEntity(CampaignMember member, Guid viewerUserId, Guid gameMasterId)
    {
        var resp = new CampaignMemberResponse
        {
            Id = member.Id,
            User = new CampaignUserInfo
            {
                Id = member.User.Id,
                Username = member.User.Username,
                AvatarId = member.User.AvatarId
            },
            JoinedAt = member.JoinedAt,
            CharacterVisibility = member.CharacterVisibility.ToString()
        };

        if (member.Character == null || member.CharacterId == null)
        {
            resp.Character = null;
            return resp;
        }

        var isOwner = member.UserId == viewerUserId;
        var isGm = viewerUserId == gameMasterId;
        var vis = member.CharacterVisibility;

        // Owner always sees full info
        if (isOwner)
        {
            resp.Character = CampaignCharacterInfo.Full(member.Character);
            return resp;
        }

        switch (vis)
        {
            case Database.Enums.CharacterVisibility.Private:
                // Nobody else sees anything
                resp.Character = CampaignCharacterInfo.Hidden();
                break;
            case Database.Enums.CharacterVisibility.CampaignPrivate:
                // GM sees full, others see hidden
                resp.Character = isGm
                    ? CampaignCharacterInfo.Full(member.Character)
                    : CampaignCharacterInfo.Hidden();
                break;
            case Database.Enums.CharacterVisibility.SemiPublic:
                // GM sees full, others see name+avatar only
                resp.Character = isGm
                    ? CampaignCharacterInfo.Full(member.Character)
                    : CampaignCharacterInfo.Summary(member.Character);
                break;
            case Database.Enums.CharacterVisibility.Public:
                // Everyone sees full
                resp.Character = CampaignCharacterInfo.Full(member.Character);
                break;
        }

        return resp;
    }
}

public sealed class CampaignCharacterInfo
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public Guid? AvatarId { get; set; }
    public int? Level { get; set; }
    public bool IsHidden { get; set; }

    public static CampaignCharacterInfo Full(Character character) => new()
    {
        Id = character.Id,
        Name = character.Name,
        AvatarId = character.AvatarId,
        Level = character.Level,
        IsHidden = false
    };

    public static CampaignCharacterInfo Summary(Character character) => new()
    {
        Id = character.Id,
        Name = character.Name,
        AvatarId = character.AvatarId,
        Level = null,
        IsHidden = false
    };

    public static CampaignCharacterInfo Hidden() => new()
    {
        IsHidden = true
    };
}
