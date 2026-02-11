using Dragonwright.Database;
using Dragonwright.Database.Entities;
using Dragonwright.Database.Enums;
using Dragonwright.Models;
using Dragonwright.Models.Characters;
using Dragonwright.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Controllers;

/// <summary>
/// Controller for managing characters and all their related data.
/// </summary>
[Authorize]
[ApiController]
[Route("characters")]
public sealed class CharactersController(AppDbContext dbContext, FileStorageService fileStorageService) : CharacterControllerBase(dbContext)
{
    #region Base Character CRUD

    /// <summary>
    /// Gets a paginated list of the current user's characters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListCharacters(int page = 1, int pageSize = 20, string? search = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        (page, pageSize) = NormalizePagination(page, pageSize);

        var query = DbContext.Characters
            .Where(c => c.UserId == userId.Value)
            .Include(c => c.Avatar)
            .Include(c => c.Classes).ThenInclude(cc => cc.Class)
            .Include(c => c.Race).ThenInclude(r => r!.Race)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

        var totalCount = await query.CountAsync();
        var characters = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Character>
        {
            Items = characters,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Gets a character by ID with full details.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCharacter(Guid id)
    {
        var character = await DbContext.Characters
            .Include(c => c.Avatar)
            .Include(c => c.Race).ThenInclude(r => r!.Race).ThenInclude(r => r.Traits)
            .Include(c => c.Background).ThenInclude(b => b!.Background)
            .Include(c => c.Classes).ThenInclude(cc => cc.Class)
            .Include(c => c.Classes).ThenInclude(cc => cc.Subclass)
            .Include(c => c.Abilities)
            .Include(c => c.Skills)
            .Include(c => c.Feats).ThenInclude(f => f.Feat)
            .Include(c => c.Spells).ThenInclude(s => s.Spell)
            .Include(c => c.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character == null) return NotFound();
        if (!await HasCharacterAccessAsync(character)) return Forbid();

        return Ok(character);
    }

    /// <summary>
    /// Creates a new character with default values.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var character = new Character
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            Name = request.Name,
            Sources = [SourceType.One2024, SourceType.Legacy2014],
            AdvancementType = AdvancementType.Milestone,
            HitPointType = HitPointType.Fixed,
            AbilityScoreGenerationMethod = AbilityScoreGenerationMethod.PointBuy,
            AllowMulticlassing = true,
            CheckMulticlassingPrerequisites = true,
            BaseArmorClass = 10,
            MovementSpeed = 30,
            Size = Size.Medium
        };

        DbContext.Characters.Add(character);

        foreach (var ability in Enum.GetValues<AbilityScore>())
        {
            var characterAbility = new CharacterAbility
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                Ability = ability,
                RawScore = 8
            };
            DbContext.CharacterAbilities.Add(characterAbility);
        }

        foreach (var skill in Enum.GetValues<Skill>())
        {
            var characterSkill = new CharacterSkill
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                Skill = skill,
                RawProficiency = Proficiency.NotProficient
            };
            DbContext.CharacterSkills.Add(characterSkill);
        }

        await DbContext.SaveChangesAsync();

        var createdCharacter = await DbContext.Characters
            .Include(c => c.Abilities)
            .Include(c => c.Skills)
            .FirstAsync(c => c.Id == character.Id);

        return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, createdCharacter);
    }

    /// <summary>
    /// Uploads or replaces a character's avatar image. The image is stored using the file storage service, and the character's AvatarId is updated accordingly.
    /// If an old avatar exists, it is deleted from storage after the new one is saved.
    /// </summary>
    [HttpPut("{id:guid}/avatar")]
    [ProducesResponseType(typeof(StoredFile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadCharacterAvatar(Guid id, IFormFile file)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var character = await DbContext.Characters.FindAsync(id);
        if (character == null) return NotFound();

        var oldAvatarId = character.AvatarId;

        await using var stream = file.OpenReadStream();
        var storedFile = await fileStorageService.StoreAsync(stream, file.FileName, file.ContentType);

        character.AvatarId = storedFile.Id;
        await DbContext.SaveChangesAsync();

        if (oldAvatarId.HasValue)
        {
            await fileStorageService.DeleteAsync(oldAvatarId.Value);
        }

        return Ok(storedFile);
    }

    /// <summary>
    /// Updates a character's base information.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCharacter(Guid id, [FromBody] UpdateCharacterRequest request)
    {
        var character = await DbContext.Characters.FindAsync(id);
        if (character == null) return NotFound();
        if (!await CanModifyCharacterAsync(character)) return Forbid();

        character.Name = request.Name;
        character.AvatarId = request.AvatarId;
        character.Sources = request.Sources;
        character.AdvancementType = request.AdvancementType;
        character.HitPointType = request.HitPointType;
        character.AbilityScoreGenerationMethod = request.AbilityScoreGenerationMethod;
        character.OptionalClassFeatures = request.OptionalClassFeatures;
        character.CustomizeOrigin = request.CustomizeOrigin;
        character.ExceedLevelCap = request.ExceedLevelCap;
        character.AllowMulticlassing = request.AllowMulticlassing;
        character.CheckMulticlassingPrerequisites = request.CheckMulticlassingPrerequisites;
        character.MovementSpeed = request.MovementSpeed;
        character.SwimmingSpeed = request.SwimmingSpeed;
        character.FlyingSpeed = request.FlyingSpeed;
        character.InspirationPoints = request.InspirationPoints;
        character.MaxHitDie = request.MaxHitDie;
        character.CurrentHitDie = request.CurrentHitDie;
        character.TemporaryHitPoints = request.TemporaryHitPoints;
        character.CurrentHitPoints = request.CurrentHitPoints;
        character.RawMaximumHitPoints = request.RawMaximumHitPoints;
        character.HitPointBonus = request.HitPointBonus;
        character.OverriddenMaximumHitPoints = request.OverriddenMaximumHitPoints;
        character.InitiativeBonus = request.InitiativeBonus;
        character.BaseArmorClass = request.BaseArmorClass;
        character.ArmorClassBonus = request.ArmorClassBonus;
        character.PassivePerceptionBonus = request.PassivePerceptionBonus;
        character.PassiveInvestigationBonus = request.PassiveInvestigationBonus;
        character.PassiveInsightBonus = request.PassiveInsightBonus;
        character.XP = request.XP;
        character.DeathSaveSuccesses = request.DeathSaveSuccesses;
        character.DeathSaveFailures = request.DeathSaveFailures;
        character.ExhaustionLevel = request.ExhaustionLevel;
        character.Conditions = request.Conditions;
        character.DamageDefenses = request.DamageDefenses;
        character.ConditionDefenses = request.ConditionDefenses;
        character.SavingThrowAdvantages = request.SavingThrowAdvantages;
        character.SavingThrowDisadvantages = request.SavingThrowDisadvantages;
        character.BlindsightRange = request.BlindsightRange;
        character.BlindsightNote = request.BlindsightNote;
        character.DarkvisionRange = request.DarkvisionRange;
        character.DarkvisionNote = request.DarkvisionNote;
        character.TremorsenseRange = request.TremorsenseRange;
        character.TremorsenseNote = request.TremorsenseNote;
        character.TruesightRange = request.TruesightRange;
        character.TruesightNote = request.TruesightNote;
        character.Languages = request.Languages;
        character.ArmorProficiencies = request.ArmorProficiencies;
        character.WeaponProficiencies = request.WeaponProficiencies;
        character.ToolProficiencies = request.ToolProficiencies;
        character.CountMoneyWeight = request.CountMoneyWeight;
        character.Gold = request.Gold;
        character.Electrum = request.Electrum;
        character.Silver = request.Silver;
        character.Copper = request.Copper;
        character.ArrowQuiver = request.ArrowQuiver;
        character.BoltQuiver = request.BoltQuiver;
        character.Lifestyle = request.Lifestyle;
        character.Alignment = request.Alignment;
        character.Gender = request.Gender;
        character.Size = request.Size;
        character.Age = request.Age;
        character.HeightInInches = request.HeightInInches;
        character.WeightInPounds = request.WeightInPounds;
        character.Skin = request.Skin;
        character.Hair = request.Hair;
        character.Eyes = request.Eyes;
        character.Appearance = request.Appearance;
        character.Faith = request.Faith;
        character.PersonalityTraits = request.PersonalityTraits;
        character.Ideals = request.Ideals;
        character.Bonds = request.Bonds;
        character.Flaws = request.Flaws;
        character.Organizations = request.Organizations;
        character.Allies = request.Allies;
        character.Enemies = request.Enemies;
        character.Backstory = request.Backstory;
        character.Notes = request.Notes;

        await DbContext.SaveChangesAsync();
        return Ok(character);
    }

    /// <summary>
    /// Deletes a character.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCharacter(Guid id)
    {
        var character = await DbContext.Characters.FindAsync(id);
        if (character == null) return NotFound();
        if (!await CanModifyCharacterAsync(character)) return Forbid();

        DbContext.Characters.Remove(character);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    #endregion

    #region Race

    /// <summary>
    /// Sets or removes a character's race.
    /// </summary>
    [HttpPut("{id:guid}/race")]
    public async Task<IActionResult> SetRace(Guid id, [FromBody] SetCharacterRaceRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var existingRace = await DbContext.CharacterRaces
            .FirstOrDefaultAsync(r => r.CharacterId == id);

        if (request.RaceId == null)
        {
            if (existingRace != null)
            {
                DbContext.CharacterRaces.Remove(existingRace);
                await DbContext.SaveChangesAsync();
            }
            return Ok();
        }

        var raceEntity = await DbContext.Races.FindAsync(request.RaceId.Value);
        if (raceEntity == null) return BadRequest("Race not found");

        if (existingRace != null)
        {
            existingRace.RaceId = request.RaceId.Value;
            existingRace.RaceTraitUsages = request.RaceTraitUsages;
            existingRace.ChosenTraitOptions = request.ChosenTraitOptions;
            existingRace.ChosenSpells = request.ChosenSpells;
        }
        else
        {
            existingRace = new CharacterRace
            {
                Id = Guid.NewGuid(),
                CharacterId = id,
                RaceId = request.RaceId.Value,
                RaceTraitUsages = request.RaceTraitUsages,
                ChosenTraitOptions = request.ChosenTraitOptions,
                ChosenSpells = request.ChosenSpells
            };
            DbContext.CharacterRaces.Add(existingRace);
        }

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterRaces
            .Include(r => r.Race).ThenInclude(r => r.Traits)
            .FirstAsync(r => r.Id == existingRace.Id);

        return Ok(updated);
    }

    #endregion

    #region Background

    /// <summary>
    /// Sets or removes a character's background.
    /// </summary>
    [HttpPut("{id:guid}/background")]
    public async Task<IActionResult> SetBackground(Guid id, [FromBody] SetCharacterBackgroundRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var existingBackground = await DbContext.CharacterBackgrounds
            .FirstOrDefaultAsync(b => b.CharacterId == id);

        if (request.BackgroundId == null)
        {
            if (existingBackground != null)
            {
                DbContext.CharacterBackgrounds.Remove(existingBackground);
                await DbContext.SaveChangesAsync();
            }
            return Ok();
        }

        var backgroundEntity = await DbContext.Backgrounds.FindAsync(request.BackgroundId.Value);
        if (backgroundEntity == null) return BadRequest("Background not found");

        if (existingBackground != null)
        {
            existingBackground.BackgroundId = request.BackgroundId.Value;
            existingBackground.ChosenAbilityScoreIncreases = request.ChosenAbilityScoreIncreases;
            existingBackground.ChosenLanguages = request.ChosenLanguages;
            existingBackground.ChosenCharacteristics = request.ChosenCharacteristics;
        }
        else
        {
            existingBackground = new CharacterBackground
            {
                Id = Guid.NewGuid(),
                CharacterId = id,
                BackgroundId = request.BackgroundId.Value,
                ChosenAbilityScoreIncreases = request.ChosenAbilityScoreIncreases,
                ChosenLanguages = request.ChosenLanguages,
                ChosenCharacteristics = request.ChosenCharacteristics
            };
            DbContext.CharacterBackgrounds.Add(existingBackground);
        }

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterBackgrounds
            .Include(b => b.Background)
            .FirstAsync(b => b.Id == existingBackground.Id);

        return Ok(updated);
    }

    #endregion

    #region Classes

    /// <summary>
    /// Sets all of a character's classes. Replaces existing classes entirely.
    /// </summary>
    [HttpPut("{id:guid}/classes")]
    public async Task<IActionResult> SetClasses(Guid id, [FromBody] SetCharacterClassesRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var existingClasses = await DbContext.CharacterClasses
            .Where(c => c.CharacterId == id)
            .ToListAsync();

        var existingIds = existingClasses.Select(c => c.Id).ToHashSet();
        var requestIds = request.Classes.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToHashSet();

        var toRemove = existingClasses.Where(c => !requestIds.Contains(c.Id)).ToList();
        DbContext.CharacterClasses.RemoveRange(toRemove);

        foreach (var classData in request.Classes)
        {
            var classEntity = await DbContext.Classes.FindAsync(classData.ClassId);
            if (classEntity == null) return BadRequest($"Class {classData.ClassId} not found");

            if (classData.SubclassId.HasValue)
            {
                var subclass = await DbContext.Subclasses.FindAsync(classData.SubclassId.Value);
                if (subclass == null || subclass.ClassId != classData.ClassId)
                    return BadRequest($"Invalid subclass {classData.SubclassId} for class {classData.ClassId}");
            }

            if (classData.Id.HasValue && existingIds.Contains(classData.Id.Value))
            {
                var existing = existingClasses.First(c => c.Id == classData.Id.Value);
                existing.ClassId = classData.ClassId;
                existing.Level = classData.Level;
                existing.SubclassId = classData.SubclassId;
                existing.IsStartingClass = classData.IsStartingClass;
                existing.ClassFeatureUsages = classData.ClassFeatureUsages;
                existing.ChosenSkillProficiencies = classData.ChosenSkillProficiencies;
                existing.ChosenFeatureOptions = classData.ChosenFeatureOptions;
                existing.ChosenSpells = classData.ChosenSpells;
                existing.SpellSlotsUsed = classData.SpellSlotsUsed;
                existing.PactSlotsUsed = classData.PactSlotsUsed;
            }
            else
            {
                var newClass = new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    CharacterId = id,
                    ClassId = classData.ClassId,
                    Level = classData.Level,
                    SubclassId = classData.SubclassId,
                    IsStartingClass = classData.IsStartingClass,
                    ClassFeatureUsages = classData.ClassFeatureUsages,
                    ChosenSkillProficiencies = classData.ChosenSkillProficiencies,
                    ChosenFeatureOptions = classData.ChosenFeatureOptions,
                    ChosenSpells = classData.ChosenSpells,
                    SpellSlotsUsed = classData.SpellSlotsUsed,
                    PactSlotsUsed = classData.PactSlotsUsed
                };
                DbContext.CharacterClasses.Add(newClass);
            }
        }

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterClasses
            .Include(c => c.Class)
            .Include(c => c.Subclass)
            .Where(c => c.CharacterId == id)
            .OrderByDescending(c => c.IsStartingClass)
            .ThenByDescending(c => c.Level)
            .ToListAsync();

        return Ok(updated);
    }

    #endregion

    #region Abilities

    /// <summary>
    /// Sets all of a character's ability scores.
    /// </summary>
    [HttpPut("{id:guid}/abilities")]
    public async Task<IActionResult> SetAbilities(Guid id, [FromBody] SetCharacterAbilitiesRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var existingAbilities = await DbContext.CharacterAbilities
            .Where(a => a.CharacterId == id)
            .ToListAsync();

        foreach (var abilityData in request.Abilities)
        {
            var existing = existingAbilities.FirstOrDefault(a => a.Ability == abilityData.Ability);
            if (existing != null)
            {
                existing.RawScore = abilityData.RawScore;
                existing.ScoreBonus = abilityData.ScoreBonus;
                existing.RawSavingThrowProficiency = abilityData.RawSavingThrowProficiency;
                existing.OverrideSavingThrowProficiency = abilityData.OverrideSavingThrowProficiency;
                existing.SavingThrowBonus = abilityData.SavingThrowBonus;
            }
        }

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterAbilities
            .Where(a => a.CharacterId == id)
            .OrderBy(a => a.Ability)
            .ToListAsync();

        return Ok(updated);
    }

    #endregion

    #region Skills

    /// <summary>
    /// Sets all of a character's skills.
    /// </summary>
    [HttpPut("{id:guid}/skills")]
    public async Task<IActionResult> SetSkills(Guid id, [FromBody] SetCharacterSkillsRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var existingSkills = await DbContext.CharacterSkills
            .Where(s => s.CharacterId == id)
            .ToListAsync();

        foreach (var skillData in request.Skills)
        {
            var existing = existingSkills.FirstOrDefault(s => s.Skill == skillData.Skill);
            if (existing != null)
            {
                existing.Bonus = skillData.Bonus;
                existing.RawProficiency = skillData.RawProficiency;
                existing.OverrideProficiency = skillData.OverrideProficiency;
                existing.AdvantageState = skillData.AdvantageState;
            }
        }

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterSkills
            .Where(s => s.CharacterId == id)
            .OrderBy(s => s.Skill)
            .ToListAsync();

        return Ok(updated);
    }

    #endregion

    #region Feats

    /// <summary>
    /// Adds a feat to a character.
    /// </summary>
    [HttpPost("{id:guid}/feats")]
    public async Task<IActionResult> AddFeat(Guid id, [FromBody] CharacterFeatRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var featEntity = await DbContext.Feats.FindAsync(request.FeatId);
        if (featEntity == null) return BadRequest("Feat not found");

        var characterFeat = new CharacterFeat
        {
            Id = Guid.NewGuid(),
            CharacterId = id,
            FeatId = request.FeatId,
            Source = request.Source,
            SourceId = request.SourceId,
            ChosenAbilityScoreIncrease = request.ChosenAbilityScoreIncrease,
            ChosenOptions = request.ChosenOptions,
            ChosenSpells = request.ChosenSpells
        };

        DbContext.CharacterFeats.Add(characterFeat);
        await DbContext.SaveChangesAsync();

        var created = await DbContext.CharacterFeats
            .Include(f => f.Feat)
            .FirstAsync(f => f.Id == characterFeat.Id);

        return CreatedAtAction(nameof(GetCharacter), new { id }, created);
    }

    /// <summary>
    /// Updates a character's feat.
    /// </summary>
    [HttpPut("{id:guid}/feats/{featId:guid}")]
    public async Task<IActionResult> UpdateFeat(Guid id, Guid featId, [FromBody] CharacterFeatRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var characterFeat = await DbContext.CharacterFeats
            .FirstOrDefaultAsync(f => f.Id == featId && f.CharacterId == id);

        if (characterFeat == null) return NotFound();

        characterFeat.FeatId = request.FeatId;
        characterFeat.Source = request.Source;
        characterFeat.SourceId = request.SourceId;
        characterFeat.ChosenAbilityScoreIncrease = request.ChosenAbilityScoreIncrease;
        characterFeat.ChosenOptions = request.ChosenOptions;
        characterFeat.ChosenSpells = request.ChosenSpells;

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterFeats
            .Include(f => f.Feat)
            .FirstAsync(f => f.Id == featId);

        return Ok(updated);
    }

    /// <summary>
    /// Removes a feat from a character.
    /// </summary>
    [HttpDelete("{id:guid}/feats/{featId:guid}")]
    public async Task<IActionResult> RemoveFeat(Guid id, Guid featId)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var feat = await DbContext.CharacterFeats
            .FirstOrDefaultAsync(f => f.Id == featId && f.CharacterId == id);

        if (feat == null) return NotFound();

        DbContext.CharacterFeats.Remove(feat);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    #endregion

    #region Spells

    /// <summary>
    /// Adds a spell to a character.
    /// </summary>
    [HttpPost("{id:guid}/spells")]
    public async Task<IActionResult> AddSpell(Guid id, [FromBody] AddCharacterSpellRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var spellEntity = await DbContext.Spells.FindAsync(request.SpellId);
        if (spellEntity == null) return BadRequest("Spell not found");

        if (request.SourceClassId.HasValue)
        {
            var sourceClass = await DbContext.Classes.FindAsync(request.SourceClassId.Value);
            if (sourceClass == null) return BadRequest("Source class not found");
        }

        var characterSpell = new CharacterSpell
        {
            Id = Guid.NewGuid(),
            CharacterId = id,
            SpellId = request.SpellId,
            SpellSource = request.SpellSource,
            SourceClassId = request.SourceClassId,
            IsPrepared = request.IsPrepared,
            AlwaysPrepared = request.AlwaysPrepared,
            CastAtLevelOverride = request.CastAtLevelOverride,
            UsesRemaining = request.MaxUses,
            MaxUses = request.MaxUses,
            ResetType = request.ResetType
        };

        DbContext.CharacterSpells.Add(characterSpell);
        await DbContext.SaveChangesAsync();

        var created = await DbContext.CharacterSpells
            .Include(s => s.Spell)
            .Include(s => s.SourceClass)
            .FirstAsync(s => s.Id == characterSpell.Id);

        return CreatedAtAction(nameof(GetCharacter), new { id }, created);
    }

    /// <summary>
    /// Updates a character's spell.
    /// </summary>
    [HttpPut("{id:guid}/spells/{spellId:guid}")]
    public async Task<IActionResult> UpdateSpell(Guid id, Guid spellId, [FromBody] UpdateCharacterSpellRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var characterSpell = await DbContext.CharacterSpells
            .FirstOrDefaultAsync(s => s.Id == spellId && s.CharacterId == id);

        if (characterSpell == null) return NotFound();

        characterSpell.IsPrepared = request.IsPrepared;
        characterSpell.UsesRemaining = request.UsesRemaining;

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterSpells
            .Include(s => s.Spell)
            .Include(s => s.SourceClass)
            .FirstAsync(s => s.Id == spellId);

        return Ok(updated);
    }

    /// <summary>
    /// Removes a spell from a character.
    /// </summary>
    [HttpDelete("{id:guid}/spells/{spellId:guid}")]
    public async Task<IActionResult> RemoveSpell(Guid id, Guid spellId)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var spell = await DbContext.CharacterSpells
            .FirstOrDefaultAsync(s => s.Id == spellId && s.CharacterId == id);

        if (spell == null) return NotFound();

        DbContext.CharacterSpells.Remove(spell);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    #endregion

    #region Items

    /// <summary>
    /// Adds an item to a character's inventory.
    /// </summary>
    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddCharacterItemRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var itemEntity = await DbContext.Items.FindAsync(request.ItemId);
        if (itemEntity == null) return BadRequest("Item not found");

        var characterItem = new CharacterItem
        {
            Id = Guid.NewGuid(),
            CharacterId = id,
            ItemId = request.ItemId,
            Quantity = request.Quantity,
            Notes = request.Notes,
            Attuned = request.Attuned,
            Equipped = request.Equipped
        };

        DbContext.CharacterItems.Add(characterItem);
        await DbContext.SaveChangesAsync();

        var created = await DbContext.CharacterItems
            .Include(i => i.Item)
            .FirstAsync(i => i.Id == characterItem.Id);

        return CreatedAtAction(nameof(GetCharacter), new { id }, created);
    }

    /// <summary>
    /// Updates a character's inventory item.
    /// </summary>
    [HttpPut("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateCharacterItemRequest request)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var characterItem = await DbContext.CharacterItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.CharacterId == id);

        if (characterItem == null) return NotFound();

        characterItem.Quantity = request.Quantity;
        characterItem.Notes = request.Notes;
        characterItem.Attuned = request.Attuned;
        characterItem.Equipped = request.Equipped;

        await DbContext.SaveChangesAsync();

        var updated = await DbContext.CharacterItems
            .Include(i => i.Item)
            .FirstAsync(i => i.Id == itemId);

        return Ok(updated);
    }

    /// <summary>
    /// Removes an item from a character's inventory.
    /// </summary>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        if (!await CanModifyCharacterAsync(id)) return Forbid();

        var item = await DbContext.CharacterItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.CharacterId == id);

        if (item == null) return NotFound();

        DbContext.CharacterItems.Remove(item);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    #endregion
}
