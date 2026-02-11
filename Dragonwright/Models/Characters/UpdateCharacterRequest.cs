using Dragonwright.Database.Enums;

namespace Dragonwright.Models.Characters;

/// <summary>
/// Request model for updating a character.
/// </summary>
public sealed class UpdateCharacterRequest
{
    /// <summary>
    /// The name of the character.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The avatar file ID.
    /// </summary>
    public Guid? AvatarId { get; init; }

    /// <summary>
    /// The allowed content sources for this character.
    /// </summary>
    public ICollection<SourceType> Sources { get; init; } = [];

    /// <summary>
    /// The advancement type (XP or Milestone).
    /// </summary>
    public AdvancementType AdvancementType { get; init; }

    /// <summary>
    /// The hit point calculation type.
    /// </summary>
    public HitPointType HitPointType { get; init; }

    /// <summary>
    /// The ability score generation method.
    /// </summary>
    public AbilityScoreGenerationMethod AbilityScoreGenerationMethod { get; init; }

    /// <summary>
    /// Whether to use optional class features.
    /// </summary>
    public bool OptionalClassFeatures { get; init; }

    /// <summary>
    /// Whether to customize origin (Tasha's rules).
    /// </summary>
    public bool CustomizeOrigin { get; init; }

    /// <summary>
    /// Whether to allow exceeding the level 20 cap.
    /// </summary>
    public bool ExceedLevelCap { get; init; }

    /// <summary>
    /// Whether to allow multiclassing.
    /// </summary>
    public bool AllowMulticlassing { get; init; }

    /// <summary>
    /// Whether to check multiclassing prerequisites.
    /// </summary>
    public bool CheckMulticlassingPrerequisites { get; init; }

    /// <summary>
    /// The base movement speed in feet.
    /// </summary>
    public int MovementSpeed { get; init; }

    /// <summary>
    /// The swimming speed in feet.
    /// </summary>
    public int SwimmingSpeed { get; init; }

    /// <summary>
    /// The flying speed in feet.
    /// </summary>
    public int FlyingSpeed { get; init; }

    /// <summary>
    /// The number of inspiration points.
    /// </summary>
    public int InspirationPoints { get; init; }

    /// <summary>
    /// The maximum number of hit dice.
    /// </summary>
    public int MaxHitDie { get; init; }

    /// <summary>
    /// The current number of hit dice available.
    /// </summary>
    public int CurrentHitDie { get; init; }

    /// <summary>
    /// The temporary hit points.
    /// </summary>
    public int TemporaryHitPoints { get; init; }

    /// <summary>
    /// The current hit points.
    /// </summary>
    public int CurrentHitPoints { get; init; }

    /// <summary>
    /// The raw maximum hit points before bonuses.
    /// </summary>
    public int RawMaximumHitPoints { get; init; }

    /// <summary>
    /// The bonus to maximum hit points.
    /// </summary>
    public int HitPointBonus { get; init; }

    /// <summary>
    /// The overridden maximum hit points.
    /// </summary>
    public int? OverriddenMaximumHitPoints { get; init; }

    /// <summary>
    /// The initiative bonus.
    /// </summary>
    public int InitiativeBonus { get; init; }

    /// <summary>
    /// The base armor class.
    /// </summary>
    public int BaseArmorClass { get; init; }

    /// <summary>
    /// The armor class bonus.
    /// </summary>
    public int ArmorClassBonus { get; init; }

    /// <summary>
    /// The passive perception bonus.
    /// </summary>
    public int PassivePerceptionBonus { get; init; }

    /// <summary>
    /// The passive investigation bonus.
    /// </summary>
    public int PassiveInvestigationBonus { get; init; }

    /// <summary>
    /// The passive insight bonus.
    /// </summary>
    public int PassiveInsightBonus { get; init; }

    /// <summary>
    /// The experience points.
    /// </summary>
    public int XP { get; init; }

    /// <summary>
    /// The number of death save successes.
    /// </summary>
    public int DeathSaveSuccesses { get; init; }

    /// <summary>
    /// The number of death save failures.
    /// </summary>
    public int DeathSaveFailures { get; init; }

    /// <summary>
    /// The current exhaustion level (0-6).
    /// </summary>
    public int ExhaustionLevel { get; init; }

    /// <summary>
    /// The active conditions affecting the character.
    /// </summary>
    public ICollection<Condition> Conditions { get; init; } = [];

    /// <summary>
    /// The damage type defenses (resistance, immunity, vulnerability).
    /// </summary>
    public IDictionary<DamageType, List<DefenseState>> DamageDefenses { get; init; } = new Dictionary<DamageType, List<DefenseState>>();

    /// <summary>
    /// The condition defenses.
    /// </summary>
    public IDictionary<Condition, List<DefenseState>> ConditionDefenses { get; init; } = new Dictionary<Condition, List<DefenseState>>();

    /// <summary>
    /// The saving throw advantages.
    /// </summary>
    public ICollection<string> SavingThrowAdvantages { get; init; } = [];

    /// <summary>
    /// The saving throw disadvantages.
    /// </summary>
    public ICollection<string> SavingThrowDisadvantages { get; init; } = [];

    /// <summary>
    /// The blindsight range in feet.
    /// </summary>
    public int BlindsightRange { get; init; }

    /// <summary>
    /// Notes about blindsight.
    /// </summary>
    public string BlindsightNote { get; init; } = string.Empty;

    /// <summary>
    /// The darkvision range in feet.
    /// </summary>
    public int DarkvisionRange { get; init; }

    /// <summary>
    /// Notes about darkvision.
    /// </summary>
    public string DarkvisionNote { get; init; } = string.Empty;

    /// <summary>
    /// The tremorsense range in feet.
    /// </summary>
    public int TremorsenseRange { get; init; }

    /// <summary>
    /// Notes about tremorsense.
    /// </summary>
    public string TremorsenseNote { get; init; } = string.Empty;

    /// <summary>
    /// The truesight range in feet.
    /// </summary>
    public int TruesightRange { get; init; }

    /// <summary>
    /// Notes about truesight.
    /// </summary>
    public string TruesightNote { get; init; } = string.Empty;

    /// <summary>
    /// The known languages.
    /// </summary>
    public ICollection<string> Languages { get; init; } = [];

    /// <summary>
    /// The armor type proficiencies.
    /// </summary>
    public ICollection<ItemType> ArmorProficiencies { get; init; } = [];

    /// <summary>
    /// The weapon type proficiencies.
    /// </summary>
    public ICollection<WeaponType> WeaponProficiencies { get; init; } = [];

    /// <summary>
    /// The tool proficiencies.
    /// </summary>
    public ICollection<Tool> ToolProficiencies { get; init; } = [];

    /// <summary>
    /// Whether to count money weight in encumbrance.
    /// </summary>
    public bool CountMoneyWeight { get; init; }

    /// <summary>
    /// The gold pieces.
    /// </summary>
    public int Gold { get; init; }

    /// <summary>
    /// The electrum pieces.
    /// </summary>
    public int Electrum { get; init; }

    /// <summary>
    /// The silver pieces.
    /// </summary>
    public int Silver { get; init; }

    /// <summary>
    /// The copper pieces.
    /// </summary>
    public int Copper { get; init; }

    /// <summary>
    /// The number of arrows in the quiver.
    /// </summary>
    public int ArrowQuiver { get; init; }

    /// <summary>
    /// The number of bolts in the quiver.
    /// </summary>
    public int BoltQuiver { get; init; }

    /// <summary>
    /// The character's lifestyle.
    /// </summary>
    public Lifestyle Lifestyle { get; init; }

    /// <summary>
    /// The character's alignment.
    /// </summary>
    public Alignment Alignment { get; init; }

    /// <summary>
    /// The character's gender.
    /// </summary>
    public Gender Gender { get; init; }

    /// <summary>
    /// The character's size.
    /// </summary>
    public Size Size { get; init; }

    /// <summary>
    /// The character's age in years.
    /// </summary>
    public int Age { get; init; }

    /// <summary>
    /// The character's height in inches.
    /// </summary>
    public int HeightInInches { get; init; }

    /// <summary>
    /// The character's weight in pounds.
    /// </summary>
    public int WeightInPounds { get; init; }

    /// <summary>
    /// The character's skin description.
    /// </summary>
    public string Skin { get; init; } = string.Empty;

    /// <summary>
    /// The character's hair description.
    /// </summary>
    public string Hair { get; init; } = string.Empty;

    /// <summary>
    /// The character's eye description.
    /// </summary>
    public string Eyes { get; init; } = string.Empty;

    /// <summary>
    /// The character's appearance description.
    /// </summary>
    public string Appearance { get; init; } = string.Empty;

    /// <summary>
    /// The character's faith or deity.
    /// </summary>
    public string Faith { get; init; } = string.Empty;

    /// <summary>
    /// The character's personality traits.
    /// </summary>
    public ICollection<string> PersonalityTraits { get; init; } = [];

    /// <summary>
    /// The character's ideals.
    /// </summary>
    public ICollection<string> Ideals { get; init; } = [];

    /// <summary>
    /// The character's bonds.
    /// </summary>
    public ICollection<string> Bonds { get; init; } = [];

    /// <summary>
    /// The character's flaws.
    /// </summary>
    public ICollection<string> Flaws { get; init; } = [];

    /// <summary>
    /// The organizations the character belongs to.
    /// </summary>
    public ICollection<string> Organizations { get; init; } = [];

    /// <summary>
    /// The character's allies.
    /// </summary>
    public ICollection<string> Allies { get; init; } = [];

    /// <summary>
    /// The character's enemies.
    /// </summary>
    public ICollection<string> Enemies { get; init; } = [];

    /// <summary>
    /// The character's backstory.
    /// </summary>
    public string Backstory { get; init; } = string.Empty;

    /// <summary>
    /// Additional notes about the character.
    /// </summary>
    public string Notes { get; init; } = string.Empty;
}