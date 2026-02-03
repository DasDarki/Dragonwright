using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dragonwright.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Creatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Alignment = table.Column<string>(type: "text", nullable: false),
                    ArmorClass = table.Column<int>(type: "integer", nullable: false),
                    ArmorClassNotes = table.Column<string>(type: "text", nullable: false),
                    HitPoints = table.Column<int>(type: "integer", nullable: false),
                    HitPointDiceCount = table.Column<int>(type: "integer", nullable: false),
                    HitPointDiceValue = table.Column<int>(type: "integer", nullable: false),
                    HitPointDiceBonus = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    FlyingSpeed = table.Column<int>(type: "integer", nullable: false),
                    SwimmingSpeed = table.Column<int>(type: "integer", nullable: false),
                    ClimbSpeed = table.Column<int>(type: "integer", nullable: false),
                    ChallengeRating = table.Column<float>(type: "real", nullable: false),
                    XP = table.Column<int>(type: "integer", nullable: false),
                    ProficiencyBonus = table.Column<int>(type: "integer", nullable: false),
                    StrengthScore = table.Column<int>(type: "integer", nullable: false),
                    StrengthSavingThrowBonus = table.Column<int>(type: "integer", nullable: false),
                    DexterityScore = table.Column<int>(type: "integer", nullable: false),
                    DexteritySavingThrowBonus = table.Column<int>(type: "integer", nullable: false),
                    ConstitutionScore = table.Column<int>(type: "integer", nullable: false),
                    ConstitutionSavingThrowBonus = table.Column<int>(type: "integer", nullable: false),
                    IntelligenceScore = table.Column<int>(type: "integer", nullable: false),
                    IntelligenceSavingThrowBonus = table.Column<int>(type: "integer", nullable: false),
                    WisdomScore = table.Column<int>(type: "integer", nullable: false),
                    WisdomSavingThrowBonus = table.Column<int>(type: "integer", nullable: false),
                    CharismaScore = table.Column<int>(type: "integer", nullable: false),
                    CharismaSavingThrowBonus = table.Column<int>(type: "integer", nullable: false),
                    SkillModifiers = table.Column<string>(type: "text", nullable: false),
                    PassivePerceptionBonus = table.Column<int>(type: "integer", nullable: false),
                    PassiveInvestigationBonus = table.Column<int>(type: "integer", nullable: false),
                    PassiveInsightBonus = table.Column<int>(type: "integer", nullable: false),
                    Languages = table.Column<string>(type: "text", nullable: false),
                    DarkvisionRange = table.Column<int>(type: "integer", nullable: false),
                    TruesightRange = table.Column<int>(type: "integer", nullable: false),
                    TremorsenseRange = table.Column<int>(type: "integer", nullable: false),
                    BlindsightRange = table.Column<int>(type: "integer", nullable: false),
                    Traits = table.Column<string>(type: "text", nullable: false),
                    Actions = table.Column<string>(type: "text", nullable: false),
                    LegendaryActions = table.Column<string>(type: "text", nullable: false),
                    LairActions = table.Column<string>(type: "text", nullable: false),
                    Reactions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    AvatarId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserRole = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_StoredFiles_AvatarId",
                        column: x => x.AvatarId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LanguageCount = table.Column<int>(type: "integer", nullable: false),
                    AbilityScoreIncreases = table.Column<string>(type: "text", nullable: false),
                    LanguageRestrictions = table.Column<string>(type: "text", nullable: false),
                    SkillProficiencies = table.Column<string>(type: "text", nullable: false),
                    ToolProficiencies = table.Column<string>(type: "text", nullable: false),
                    ArmorProficiencies = table.Column<string>(type: "text", nullable: false),
                    WeaponProficiencies = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Backgrounds_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AvatarId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sources = table.Column<string>(type: "text", nullable: false),
                    AdvancementType = table.Column<string>(type: "text", nullable: false),
                    HitPointType = table.Column<string>(type: "text", nullable: false),
                    AbilityScoreGenerationMethod = table.Column<string>(type: "text", nullable: false),
                    OptionalClassFeatures = table.Column<bool>(type: "boolean", nullable: false),
                    CustomizeOrigin = table.Column<bool>(type: "boolean", nullable: false),
                    ExceedLevelCap = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMulticlassing = table.Column<bool>(type: "boolean", nullable: false),
                    CheckMulticlassingPrerequisites = table.Column<bool>(type: "boolean", nullable: false),
                    MovementSpeed = table.Column<int>(type: "integer", nullable: false),
                    SwimmingSpeed = table.Column<int>(type: "integer", nullable: false),
                    FlyingSpeed = table.Column<int>(type: "integer", nullable: false),
                    InspirationPoints = table.Column<int>(type: "integer", nullable: false),
                    MaxHitDie = table.Column<int>(type: "integer", nullable: false),
                    CurrentHitDie = table.Column<int>(type: "integer", nullable: false),
                    TemporaryHitPoints = table.Column<int>(type: "integer", nullable: false),
                    CurrentHitPoints = table.Column<int>(type: "integer", nullable: false),
                    RawMaximumHitPoints = table.Column<int>(type: "integer", nullable: false),
                    HitPointBonus = table.Column<int>(type: "integer", nullable: false),
                    OverriddenMaximumHitPoints = table.Column<int>(type: "integer", nullable: true),
                    InitiativeBonus = table.Column<int>(type: "integer", nullable: false),
                    BaseArmorClass = table.Column<int>(type: "integer", nullable: false),
                    ArmorClassBonus = table.Column<int>(type: "integer", nullable: false),
                    PassivePerceptionBonus = table.Column<int>(type: "integer", nullable: false),
                    PassiveInvestigationBonus = table.Column<int>(type: "integer", nullable: false),
                    PassiveInsightBonus = table.Column<int>(type: "integer", nullable: false),
                    XP = table.Column<int>(type: "integer", nullable: false),
                    DeathSaveSuccesses = table.Column<int>(type: "integer", nullable: false),
                    DeathSaveFailures = table.Column<int>(type: "integer", nullable: false),
                    ExhaustionLevel = table.Column<int>(type: "integer", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: false),
                    DamageDefenses = table.Column<string>(type: "text", nullable: false),
                    ConditionDefenses = table.Column<string>(type: "text", nullable: false),
                    SavingThrowAdvantages = table.Column<string>(type: "text", nullable: false),
                    SavingThrowDisadvantages = table.Column<string>(type: "text", nullable: false),
                    BlindsightRange = table.Column<int>(type: "integer", nullable: false),
                    BlindsightNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DarkvisionRange = table.Column<int>(type: "integer", nullable: false),
                    DarkvisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TremorsenseRange = table.Column<int>(type: "integer", nullable: false),
                    TremorsenseNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TruesightRange = table.Column<int>(type: "integer", nullable: false),
                    TruesightNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Languages = table.Column<string>(type: "text", nullable: false),
                    ArmorProficiencies = table.Column<string>(type: "text", nullable: false),
                    WeaponProficiencies = table.Column<string>(type: "text", nullable: false),
                    ToolProficiencies = table.Column<string>(type: "text", nullable: false),
                    CountMoneyWeight = table.Column<bool>(type: "boolean", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    Electrum = table.Column<int>(type: "integer", nullable: false),
                    Silver = table.Column<int>(type: "integer", nullable: false),
                    Copper = table.Column<int>(type: "integer", nullable: false),
                    ArrowQuiver = table.Column<int>(type: "integer", nullable: false),
                    BoltQuiver = table.Column<int>(type: "integer", nullable: false),
                    Lifestyle = table.Column<string>(type: "text", nullable: false),
                    Alignment = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    HeightInInches = table.Column<int>(type: "integer", nullable: false),
                    WeightInPounds = table.Column<int>(type: "integer", nullable: false),
                    Skin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Hair = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Eyes = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Appearance = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Faith = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PersonalityTraits = table.Column<string>(type: "text", nullable: false),
                    Ideals = table.Column<string>(type: "text", nullable: false),
                    Bonds = table.Column<string>(type: "text", nullable: false),
                    Flaws = table.Column<string>(type: "text", nullable: false),
                    Organizations = table.Column<string>(type: "text", nullable: false),
                    Allies = table.Column<string>(type: "text", nullable: false),
                    Enemies = table.Column<string>(type: "text", nullable: false),
                    Backstory = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_StoredFiles_AvatarId",
                        column: x => x.AvatarId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Characters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HitDie = table.Column<int>(type: "integer", nullable: false),
                    FixHitPointsPerLevelAfterFirst = table.Column<int>(type: "integer", nullable: false),
                    BaseHitPointsAtFirstLevel = table.Column<int>(type: "integer", nullable: false),
                    HitPointsModifierAbilityScore = table.Column<int>(type: "integer", nullable: true),
                    PrimaryAbilityScores = table.Column<string>(type: "text", nullable: false),
                    SavingThrowProficiencies = table.Column<string>(type: "text", nullable: false),
                    SkillProficienciesCount = table.Column<int>(type: "integer", nullable: false),
                    SkillProficienciesOptions = table.Column<string>(type: "text", nullable: false),
                    ToolProficiencies = table.Column<string>(type: "text", nullable: false),
                    ArmorProficiencies = table.Column<string>(type: "text", nullable: false),
                    WeaponProficiencies = table.Column<string>(type: "text", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_StoredFiles_ImageId",
                        column: x => x.ImageId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Classes_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Feats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FeatLevel = table.Column<int>(type: "integer", nullable: false),
                    IsRepeatable = table.Column<bool>(type: "boolean", nullable: false),
                    PrerequisiteDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PrerequisiteAbilityScore = table.Column<string>(type: "text", nullable: true),
                    PrerequisiteAbilityScoreMinimum = table.Column<int>(type: "integer", nullable: false),
                    PrerequisiteSpellcasting = table.Column<bool>(type: "boolean", nullable: false),
                    AbilityScoreOptions = table.Column<string>(type: "text", nullable: false),
                    AbilityScoreIncrease = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feats_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsMagical = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAttunement = table.Column<bool>(type: "boolean", nullable: false),
                    IsConsumable = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<string>(type: "text", nullable: false),
                    WeightInOunces = table.Column<int>(type: "integer", nullable: false),
                    ValueInCopper = table.Column<int>(type: "integer", nullable: false),
                    WeaponType = table.Column<string>(type: "text", nullable: true),
                    BaseArmorClass = table.Column<int>(type: "integer", nullable: true),
                    ArmorClassBonus = table.Column<int>(type: "integer", nullable: false),
                    ArmorClassBonusAbility = table.Column<string>(type: "text", nullable: true),
                    MaximumArmorClassBonusFromAbility = table.Column<int>(type: "integer", nullable: false),
                    GivesDisadvantageOnStealth = table.Column<bool>(type: "boolean", nullable: false),
                    DonningTimeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    DoffingTimeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    RequiredAbilityScore = table.Column<string>(type: "text", nullable: true),
                    RequiredAbilityScoreValue = table.Column<int>(type: "integer", nullable: false),
                    WeaponProperties = table.Column<string>(type: "text", nullable: false),
                    RangeInFeet = table.Column<int>(type: "integer", nullable: false),
                    MaximumRangeInFeet = table.Column<int>(type: "integer", nullable: false),
                    AttackBonus = table.Column<int>(type: "integer", nullable: false),
                    Damages = table.Column<string>(type: "text", nullable: false),
                    DamageBonusAbility = table.Column<string>(type: "text", nullable: true),
                    DamageTypes = table.Column<string>(type: "text", nullable: false),
                    Mastery = table.Column<string>(type: "text", nullable: true),
                    IsBackpack = table.Column<bool>(type: "boolean", nullable: false),
                    ToolType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Races_StoredFiles_ImageId",
                        column: x => x.ImageId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Races_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    JwtId = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    TokenFamily = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AbsoluteExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    School = table.Column<string>(type: "text", nullable: false),
                    CastingTimes = table.Column<string>(type: "text", nullable: false),
                    HasVocalComponent = table.Column<bool>(type: "boolean", nullable: false),
                    HasSomaticComponent = table.Column<bool>(type: "boolean", nullable: false),
                    HasMaterialComponent = table.Column<bool>(type: "boolean", nullable: false),
                    MaterialComponents = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Concentration = table.Column<bool>(type: "boolean", nullable: false),
                    Ritual = table.Column<bool>(type: "boolean", nullable: false),
                    AttackType = table.Column<string>(type: "text", nullable: false),
                    Save = table.Column<int>(type: "integer", nullable: true),
                    Range = table.Column<int>(type: "integer", nullable: false),
                    AreaOfEffect = table.Column<int>(type: "integer", nullable: true),
                    AreaSize = table.Column<int>(type: "integer", nullable: false),
                    DamageTypes = table.Column<string>(type: "text", nullable: false),
                    Damages = table.Column<string>(type: "text", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: false),
                    Durations = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spells_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Characteristics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characteristics_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterAbilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ability = table.Column<string>(type: "text", nullable: false),
                    RawScore = table.Column<int>(type: "integer", nullable: false),
                    ScoreBonus = table.Column<int>(type: "integer", nullable: false),
                    RawSavingThrowProficiency = table.Column<string>(type: "text", nullable: false),
                    OverrideSavingThrowProficiency = table.Column<string>(type: "text", nullable: true),
                    SavingThrowBonus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterAbilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterAbilities_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterBackgrounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChosenAbilityScoreIncreases = table.Column<string>(type: "text", nullable: false),
                    ChosenLanguages = table.Column<string>(type: "text", nullable: false),
                    ChosenCharacteristics = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterBackgrounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterBackgrounds_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharacterBackgrounds_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Skill = table.Column<string>(type: "text", nullable: false),
                    Bonus = table.Column<int>(type: "integer", nullable: false),
                    RawProficiency = table.Column<string>(type: "text", nullable: false),
                    OverrideProficiency = table.Column<string>(type: "text", nullable: true),
                    AdvantageState = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartItemChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartItemChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartItemChoices_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartItemChoices_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subclasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceCreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CanCastSpells = table.Column<bool>(type: "boolean", nullable: false),
                    SpellcastingAbility = table.Column<string>(type: "text", nullable: true),
                    KnowsAllSpells = table.Column<bool>(type: "boolean", nullable: false),
                    SpellPrepareType = table.Column<string>(type: "text", nullable: true),
                    SpellLearnType = table.Column<string>(type: "text", nullable: true),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subclasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subclasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subclasses_StoredFiles_ImageId",
                        column: x => x.ImageId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Subclasses_Users_SourceCreatorId",
                        column: x => x.SourceCreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundFeat",
                columns: table => new
                {
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedFeatsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundFeat", x => new { x.BackgroundId, x.GrantedFeatsId });
                    table.ForeignKey(
                        name: "FK_BackgroundFeat_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackgroundFeat_Feats_GrantedFeatsId",
                        column: x => x.GrantedFeatsId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterFeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChosenAbilityScoreIncrease = table.Column<string>(type: "text", nullable: true),
                    ChosenOptions = table.Column<string>(type: "text", nullable: false),
                    ChosenSpells = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterFeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterFeats_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterFeats_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AttackType = table.Column<string>(type: "text", nullable: true),
                    Save = table.Column<string>(type: "text", nullable: true),
                    FixedSaveDC = table.Column<int>(type: "integer", nullable: false),
                    DiceCount = table.Column<int>(type: "integer", nullable: false),
                    DiceValue = table.Column<int>(type: "integer", nullable: false),
                    FixedValue = table.Column<int>(type: "integer", nullable: false),
                    EffectOnMiss = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EffectOnSaveSuccess = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EffectOnSaveFailure = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsUnarmedWeapon = table.Column<bool>(type: "boolean", nullable: false),
                    IsNaturalWeapon = table.Column<bool>(type: "boolean", nullable: false),
                    DamageType = table.Column<string>(type: "text", nullable: true),
                    DisplayAsAttack = table.Column<bool>(type: "boolean", nullable: false),
                    EffectByMartialArts = table.Column<bool>(type: "boolean", nullable: false),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    MaximumRange = table.Column<int>(type: "integer", nullable: true),
                    AreaOfEffect = table.Column<string>(type: "text", nullable: true),
                    AreaSize = table.Column<int>(type: "integer", nullable: false),
                    ActivationTime = table.Column<string>(type: "text", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatActions_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RequiredOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementDescription = table.Column<string>(type: "text", nullable: false),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatOptions_FeatOptions_RequiredOptionId",
                        column: x => x.RequiredOptionId,
                        principalTable: "FeatOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeatOptions_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundItem",
                columns: table => new
                {
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificWeaponProficienciesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundItem", x => new { x.BackgroundId, x.SpecificWeaponProficienciesId });
                    table.ForeignKey(
                        name: "FK_BackgroundItem_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackgroundItem_Items_SpecificWeaponProficienciesId",
                        column: x => x.SpecificWeaponProficienciesId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterItem",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificArmorProficienciesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterItem", x => new { x.CharacterId, x.SpecificArmorProficienciesId });
                    table.ForeignKey(
                        name: "FK_CharacterItem_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterItem_Items_SpecificArmorProficienciesId",
                        column: x => x.SpecificArmorProficienciesId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterItem1",
                columns: table => new
                {
                    Character1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificWeaponProficienciesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterItem1", x => new { x.Character1Id, x.SpecificWeaponProficienciesId });
                    table.ForeignKey(
                        name: "FK_CharacterItem1_Characters_Character1Id",
                        column: x => x.Character1Id,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterItem1_Items_SpecificWeaponProficienciesId",
                        column: x => x.SpecificWeaponProficienciesId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterItem2",
                columns: table => new
                {
                    Character2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificToolProficienciesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterItem2", x => new { x.Character2Id, x.SpecificToolProficienciesId });
                    table.ForeignKey(
                        name: "FK_CharacterItem2_Characters_Character2Id",
                        column: x => x.Character2Id,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterItem2_Items_SpecificToolProficienciesId",
                        column: x => x.SpecificToolProficienciesId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Attuned = table.Column<bool>(type: "boolean", nullable: false),
                    Equipped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterItems_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassItem",
                columns: table => new
                {
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificWeaponProficienciesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassItem", x => new { x.ClassId, x.SpecificWeaponProficienciesId });
                    table.ForeignKey(
                        name: "FK_ClassItem_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassItem_Items_SpecificWeaponProficienciesId",
                        column: x => x.SpecificWeaponProficienciesId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterRaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceTraitUsages = table.Column<string>(type: "text", nullable: false),
                    ChosenTraitOptions = table.Column<string>(type: "text", nullable: false),
                    ChosenSpells = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterRaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterRaces_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterRaces_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    HideInBuilder = table.Column<bool>(type: "boolean", nullable: false),
                    HideInCharacterSheet = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureType = table.Column<string>(type: "text", nullable: false),
                    TraitToReplaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CharactersLevelWhereOptionsArePresented = table.Column<string>(type: "text", nullable: false),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    RaceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceTraits_RaceTraits_TraitToReplaceId",
                        column: x => x.TraitToReplaceId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceTraits_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundSpell",
                columns: table => new
                {
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellListId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundSpell", x => new { x.BackgroundId, x.SpellListId });
                    table.ForeignKey(
                        name: "FK_BackgroundSpell_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackgroundSpell_Spells_SpellListId",
                        column: x => x.SpellListId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSpells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellSource = table.Column<string>(type: "text", nullable: false),
                    SourceClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    CastAtLevelOverride = table.Column<int>(type: "integer", nullable: true),
                    UsesRemaining = table.Column<int>(type: "integer", nullable: true),
                    MaxUses = table.Column<int>(type: "integer", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSpells_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSpells_Classes_SourceClassId",
                        column: x => x.SourceClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterSpells_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassSpell",
                columns: table => new
                {
                    ClassesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellListId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSpell", x => new { x.ClassesId, x.SpellListId });
                    table.ForeignKey(
                        name: "FK_ClassSpell_Classes_ClassesId",
                        column: x => x.ClassesId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSpell_Spells_SpellListId",
                        column: x => x.SpellListId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatSpell",
                columns: table => new
                {
                    FeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellListId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatSpell", x => new { x.FeatId, x.SpellListId });
                    table.ForeignKey(
                        name: "FK_FeatSpell_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatSpell_Spells_SpellListId",
                        column: x => x.SpellListId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatSpells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpellLevels = table.Column<string>(type: "text", nullable: false),
                    SpellSchools = table.Column<string>(type: "text", nullable: false),
                    AttackTypes = table.Column<string>(type: "text", nullable: false),
                    LevelDivisor = table.Column<int>(type: "integer", nullable: false),
                    OnlyRitualSpells = table.Column<bool>(type: "boolean", nullable: false),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    NumberOfUses = table.Column<int>(type: "integer", nullable: false),
                    NumberOfUsesStatModifierOperation = table.Column<string>(type: "text", nullable: true),
                    NumberOfUsesStatModifierAbility = table.Column<string>(type: "text", nullable: true),
                    NumberOfUsesProficiencyBonusIfProficient = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfUsesProficiencyOperation = table.Column<string>(type: "text", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true),
                    CastAtLevel = table.Column<int>(type: "integer", nullable: true),
                    CastingTime = table.Column<string>(type: "text", nullable: true),
                    ActivationTimeUnit = table.Column<string>(type: "text", nullable: true),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    AdditionalDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Restrictions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ConsumesSpellSlot = table.Column<bool>(type: "boolean", nullable: false),
                    CountsAsKnownSpell = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableAtCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsInfinite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatSpells_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FeatSpells_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatSpells_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    CurrencyAmount = table.Column<int>(type: "integer", nullable: true),
                    WeaponTypes = table.Column<string>(type: "text", nullable: false),
                    WeaponProperties = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartItems_StartItemChoices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "StartItemChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SubclassId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsStartingClass = table.Column<bool>(type: "boolean", nullable: false),
                    ClassFeatureUsages = table.Column<string>(type: "text", nullable: false),
                    ChosenSkillProficiencies = table.Column<string>(type: "text", nullable: false),
                    ChosenFeatureOptions = table.Column<string>(type: "text", nullable: false),
                    ChosenSpells = table.Column<string>(type: "text", nullable: false),
                    SpellSlotsUsed = table.Column<string>(type: "text", nullable: false),
                    PactSlotsUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterClasses_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharacterClasses_Subclasses_SubclassId",
                        column: x => x.SubclassId,
                        principalTable: "Subclasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    HideInBuilder = table.Column<bool>(type: "boolean", nullable: false),
                    HideInCharacterSheet = table.Column<bool>(type: "boolean", nullable: false),
                    HasOptions = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayRequiredLevel = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    FeatureType = table.Column<string>(type: "text", nullable: false),
                    FeatureToReplaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassLevelWhereOptionsArePresented = table.Column<string>(type: "text", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubclassId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeatures_ClassFeatures_FeatureToReplaceId",
                        column: x => x.FeatureToReplaceId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassFeatures_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeatures_Subclasses_SubclassId",
                        column: x => x.SubclassId,
                        principalTable: "Subclasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassSubclass",
                columns: table => new
                {
                    AdditionalSpellListsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubclassId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSubclass", x => new { x.AdditionalSpellListsId, x.SubclassId });
                    table.ForeignKey(
                        name: "FK_ClassSubclass_Classes_AdditionalSpellListsId",
                        column: x => x.AdditionalSpellListsId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubclass_Subclasses_SubclassId",
                        column: x => x.SubclassId,
                        principalTable: "Subclasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpellSubclass",
                columns: table => new
                {
                    AdditionalSpellsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubclassId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellSubclass", x => new { x.AdditionalSpellsId, x.SubclassId });
                    table.ForeignKey(
                        name: "FK_SpellSubclass_Spells_AdditionalSpellsId",
                        column: x => x.AdditionalSpellsId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSubclass_Subclasses_SubclassId",
                        column: x => x.SubclassId,
                        principalTable: "Subclasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraitActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceTraitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AttackType = table.Column<string>(type: "text", nullable: true),
                    Save = table.Column<string>(type: "text", nullable: true),
                    FixedSaveDC = table.Column<int>(type: "integer", nullable: false),
                    DiceCount = table.Column<int>(type: "integer", nullable: false),
                    DiceValue = table.Column<int>(type: "integer", nullable: false),
                    FixedValue = table.Column<int>(type: "integer", nullable: false),
                    EffectOnMiss = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EffectOnSaveSuccess = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EffectOnSaveFailure = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsUnarmedWeapon = table.Column<bool>(type: "boolean", nullable: false),
                    IsNaturalWeapon = table.Column<bool>(type: "boolean", nullable: false),
                    DamageType = table.Column<string>(type: "text", nullable: true),
                    DisplayAsAttack = table.Column<bool>(type: "boolean", nullable: false),
                    EffectByMartialArts = table.Column<bool>(type: "boolean", nullable: false),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    MaximumRange = table.Column<int>(type: "integer", nullable: true),
                    AreaOfEffect = table.Column<string>(type: "text", nullable: true),
                    AreaSize = table.Column<int>(type: "integer", nullable: false),
                    ActivationTime = table.Column<string>(type: "text", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraitActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceTraitActions_RaceTraits_RaceTraitId",
                        column: x => x.RaceTraitId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraitCreatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceTraitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatureGroup = table.Column<string>(type: "text", nullable: false),
                    ExistingCreatureId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatureType = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaxChallengeRating = table.Column<float>(type: "real", nullable: true),
                    ChallengeRatingLevelDivisor = table.Column<int>(type: "integer", nullable: true),
                    RestrictedMovementTypes = table.Column<string>(type: "text", nullable: false),
                    CreatureSizes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraitCreatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceTraitCreatures_Creatures_ExistingCreatureId",
                        column: x => x.ExistingCreatureId,
                        principalTable: "Creatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceTraitCreatures_RaceTraits_RaceTraitId",
                        column: x => x.RaceTraitId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraitOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceTraitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RequiredOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementDescription = table.Column<string>(type: "text", nullable: false),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraitOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceTraitOptions_RaceTraitOptions_RequiredOptionId",
                        column: x => x.RequiredOptionId,
                        principalTable: "RaceTraitOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceTraitOptions_RaceTraits_RaceTraitId",
                        column: x => x.RaceTraitId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraitSpell",
                columns: table => new
                {
                    RaceTraitId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellListId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraitSpell", x => new { x.RaceTraitId, x.SpellListId });
                    table.ForeignKey(
                        name: "FK_RaceTraitSpell_RaceTraits_RaceTraitId",
                        column: x => x.RaceTraitId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceTraitSpell_Spells_SpellListId",
                        column: x => x.SpellListId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraitSpells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceTraitId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpellLevels = table.Column<string>(type: "text", nullable: false),
                    SpellSchools = table.Column<string>(type: "text", nullable: false),
                    AttackTypes = table.Column<string>(type: "text", nullable: false),
                    LevelDivisor = table.Column<int>(type: "integer", nullable: false),
                    OnlyRitualSpells = table.Column<bool>(type: "boolean", nullable: false),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    NumberOfUses = table.Column<int>(type: "integer", nullable: false),
                    NumberOfUsesStatModifierOperation = table.Column<string>(type: "text", nullable: true),
                    NumberOfUsesStatModifierAbility = table.Column<string>(type: "text", nullable: true),
                    NumberOfUsesProficiencyBonusIfProficient = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfUsesProficiencyOperation = table.Column<string>(type: "text", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true),
                    CastAtLevel = table.Column<int>(type: "integer", nullable: true),
                    CastingTime = table.Column<string>(type: "text", nullable: true),
                    ActivationTimeUnit = table.Column<string>(type: "text", nullable: true),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    AdditionalDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Restrictions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ConsumesSpellSlot = table.Column<bool>(type: "boolean", nullable: false),
                    CountsAsKnownSpell = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableAtCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsInfinite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraitSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceTraitSpells_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RaceTraitSpells_RaceTraits_RaceTraitId",
                        column: x => x.RaceTraitId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceTraitSpells_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatureActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AttackType = table.Column<string>(type: "text", nullable: true),
                    Save = table.Column<string>(type: "text", nullable: true),
                    FixedSaveDC = table.Column<int>(type: "integer", nullable: false),
                    DiceCount = table.Column<int>(type: "integer", nullable: false),
                    DiceValue = table.Column<int>(type: "integer", nullable: false),
                    FixedValue = table.Column<int>(type: "integer", nullable: false),
                    EffectOnMiss = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EffectOnSaveSuccess = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EffectOnSaveFailure = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsUnarmedWeapon = table.Column<bool>(type: "boolean", nullable: false),
                    IsNaturalWeapon = table.Column<bool>(type: "boolean", nullable: false),
                    DamageType = table.Column<string>(type: "text", nullable: true),
                    DisplayAsAttack = table.Column<bool>(type: "boolean", nullable: false),
                    EffectByMartialArts = table.Column<bool>(type: "boolean", nullable: false),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    MaximumRange = table.Column<int>(type: "integer", nullable: true),
                    AreaOfEffect = table.Column<string>(type: "text", nullable: true),
                    AreaSize = table.Column<int>(type: "integer", nullable: false),
                    ActivationTime = table.Column<string>(type: "text", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatureActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeatureActions_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatureCreatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatureGroup = table.Column<string>(type: "text", nullable: false),
                    ExistingCreatureId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatureType = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaxChallengeRating = table.Column<float>(type: "real", nullable: true),
                    ChallengeRatingLevelDivisor = table.Column<int>(type: "integer", nullable: true),
                    RestrictedMovementTypes = table.Column<string>(type: "text", nullable: false),
                    CreatureSizes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatureCreatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeatureCreatures_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeatureCreatures_Creatures_ExistingCreatureId",
                        column: x => x.ExistingCreatureId,
                        principalTable: "Creatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatureLevelScales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ClassLevel = table.Column<int>(type: "integer", nullable: false),
                    DiceCount = table.Column<int>(type: "integer", nullable: false),
                    DiceValue = table.Column<int>(type: "integer", nullable: false),
                    FixedValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatureLevelScales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeatureLevelScales_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatureOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RequiredOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementDescription = table.Column<string>(type: "text", nullable: false),
                    RequiredCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatureOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeatureOptions_ClassFeatureOptions_RequiredOptionId",
                        column: x => x.RequiredOptionId,
                        principalTable: "ClassFeatureOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassFeatureOptions_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatureSpell",
                columns: table => new
                {
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellListId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatureSpell", x => new { x.ClassFeatureId, x.SpellListId });
                    table.ForeignKey(
                        name: "FK_ClassFeatureSpell_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeatureSpell_Spells_SpellListId",
                        column: x => x.SpellListId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeatureSpells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpellLevels = table.Column<string>(type: "text", nullable: false),
                    SpellSchools = table.Column<string>(type: "text", nullable: false),
                    AttackTypes = table.Column<string>(type: "text", nullable: false),
                    LevelDivisor = table.Column<int>(type: "integer", nullable: false),
                    OnlyRitualSpells = table.Column<bool>(type: "boolean", nullable: false),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    NumberOfUses = table.Column<int>(type: "integer", nullable: false),
                    NumberOfUsesStatModifierOperation = table.Column<string>(type: "text", nullable: true),
                    NumberOfUsesStatModifierAbility = table.Column<string>(type: "text", nullable: true),
                    NumberOfUsesProficiencyBonusIfProficient = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfUsesProficiencyOperation = table.Column<string>(type: "text", nullable: true),
                    ResetType = table.Column<string>(type: "text", nullable: true),
                    CastAtLevel = table.Column<int>(type: "integer", nullable: true),
                    CastingTime = table.Column<string>(type: "text", nullable: true),
                    ActivationTimeUnit = table.Column<string>(type: "text", nullable: true),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    AdditionalDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Restrictions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ConsumesSpellSlot = table.Column<bool>(type: "boolean", nullable: false),
                    CountsAsKnownSpell = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableAtCharacterLevel = table.Column<int>(type: "integer", nullable: false),
                    IsInfinite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeatureSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeatureSpells_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeatureSpells_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClassFeatureSpells_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Modifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Subtype = table.Column<string>(type: "text", nullable: true),
                    AbilityScore = table.Column<string>(type: "text", nullable: true),
                    DiceCount = table.Column<int>(type: "integer", nullable: false),
                    DiceValue = table.Column<int>(type: "integer", nullable: false),
                    FixedValue = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: true),
                    ClassFeatureId = table.Column<Guid>(type: "uuid", nullable: true),
                    FeatId = table.Column<Guid>(type: "uuid", nullable: true),
                    RaceTraitId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modifiers_ClassFeatures_ClassFeatureId",
                        column: x => x.ClassFeatureId,
                        principalTable: "ClassFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Modifiers_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Modifiers_RaceTraits_RaceTraitId",
                        column: x => x.RaceTraitId,
                        principalTable: "RaceTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundFeat_GrantedFeatsId",
                table: "BackgroundFeat",
                column: "GrantedFeatsId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundItem_SpecificWeaponProficienciesId",
                table: "BackgroundItem",
                column: "SpecificWeaponProficienciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Backgrounds_SourceCreatorId",
                table: "Backgrounds",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundSpell_SpellListId",
                table: "BackgroundSpell",
                column: "SpellListId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterAbilities_CharacterId",
                table: "CharacterAbilities",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterBackgrounds_BackgroundId",
                table: "CharacterBackgrounds",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterBackgrounds_CharacterId",
                table: "CharacterBackgrounds",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterClasses_CharacterId",
                table: "CharacterClasses",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterClasses_ClassId",
                table: "CharacterClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterClasses_SubclassId",
                table: "CharacterClasses",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterFeats_CharacterId",
                table: "CharacterFeats",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterFeats_FeatId",
                table: "CharacterFeats",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Characteristics_BackgroundId",
                table: "Characteristics",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItem_SpecificArmorProficienciesId",
                table: "CharacterItem",
                column: "SpecificArmorProficienciesId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItem1_SpecificWeaponProficienciesId",
                table: "CharacterItem1",
                column: "SpecificWeaponProficienciesId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItem2_SpecificToolProficienciesId",
                table: "CharacterItem2",
                column: "SpecificToolProficienciesId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItems_CharacterId",
                table: "CharacterItems",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItems_ItemId",
                table: "CharacterItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterRaces_CharacterId",
                table: "CharacterRaces",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterRaces_RaceId",
                table: "CharacterRaces",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_AvatarId",
                table: "Characters",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_CharacterId",
                table: "CharacterSkills",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpells_CharacterId",
                table: "CharacterSpells",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpells_SourceClassId",
                table: "CharacterSpells",
                column: "SourceClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpells_SpellId",
                table: "CharacterSpells",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ImageId",
                table: "Classes",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SourceCreatorId",
                table: "Classes",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureActions_ClassFeatureId",
                table: "ClassFeatureActions",
                column: "ClassFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureCreatures_ClassFeatureId",
                table: "ClassFeatureCreatures",
                column: "ClassFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureCreatures_ExistingCreatureId",
                table: "ClassFeatureCreatures",
                column: "ExistingCreatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureLevelScales_ClassFeatureId",
                table: "ClassFeatureLevelScales",
                column: "ClassFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureOptions_ClassFeatureId",
                table: "ClassFeatureOptions",
                column: "ClassFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureOptions_RequiredOptionId",
                table: "ClassFeatureOptions",
                column: "RequiredOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatures_ClassId",
                table: "ClassFeatures",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatures_FeatureToReplaceId",
                table: "ClassFeatures",
                column: "FeatureToReplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatures_SubclassId",
                table: "ClassFeatures",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureSpell_SpellListId",
                table: "ClassFeatureSpell",
                column: "SpellListId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureSpells_ClassFeatureId",
                table: "ClassFeatureSpells",
                column: "ClassFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureSpells_ClassId",
                table: "ClassFeatureSpells",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeatureSpells_SpellId",
                table: "ClassFeatureSpells",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassItem_SpecificWeaponProficienciesId",
                table: "ClassItem",
                column: "SpecificWeaponProficienciesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSpell_SpellListId",
                table: "ClassSpell",
                column: "SpellListId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubclass_SubclassId",
                table: "ClassSubclass",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatActions_FeatId",
                table: "FeatActions",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatOptions_FeatId",
                table: "FeatOptions",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatOptions_RequiredOptionId",
                table: "FeatOptions",
                column: "RequiredOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Feats_SourceCreatorId",
                table: "Feats",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatSpell_SpellListId",
                table: "FeatSpell",
                column: "SpellListId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatSpells_ClassId",
                table: "FeatSpells",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatSpells_FeatId",
                table: "FeatSpells",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatSpells_SpellId",
                table: "FeatSpells",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SourceCreatorId",
                table: "Items",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Modifiers_ClassFeatureId",
                table: "Modifiers",
                column: "ClassFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Modifiers_FeatId",
                table: "Modifiers",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Modifiers_RaceTraitId",
                table: "Modifiers",
                column: "RaceTraitId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_ImageId",
                table: "Races",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_SourceCreatorId",
                table: "Races",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitActions_RaceTraitId",
                table: "RaceTraitActions",
                column: "RaceTraitId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitCreatures_ExistingCreatureId",
                table: "RaceTraitCreatures",
                column: "ExistingCreatureId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitCreatures_RaceTraitId",
                table: "RaceTraitCreatures",
                column: "RaceTraitId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitOptions_RaceTraitId",
                table: "RaceTraitOptions",
                column: "RaceTraitId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitOptions_RequiredOptionId",
                table: "RaceTraitOptions",
                column: "RequiredOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraits_RaceId",
                table: "RaceTraits",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraits_TraitToReplaceId",
                table: "RaceTraits",
                column: "TraitToReplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitSpell_SpellListId",
                table: "RaceTraitSpell",
                column: "SpellListId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitSpells_ClassId",
                table: "RaceTraitSpells",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitSpells_RaceTraitId",
                table: "RaceTraitSpells",
                column: "RaceTraitId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitSpells_SpellId",
                table: "RaceTraitSpells",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamily",
                table: "RefreshTokens",
                column: "TokenFamily");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_DeviceId",
                table: "RefreshTokens",
                columns: new[] { "UserId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Spells_SourceCreatorId",
                table: "Spells",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSubclass_SubclassId",
                table: "SpellSubclass",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_StartItemChoices_BackgroundId",
                table: "StartItemChoices",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_StartItemChoices_ClassId",
                table: "StartItemChoices",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StartItems_ChoiceId",
                table: "StartItems",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_StoragePath",
                table: "StoredFiles",
                column: "StoragePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_ClassId",
                table: "Subclasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_ImageId",
                table: "Subclasses",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_SourceCreatorId",
                table: "Subclasses",
                column: "SourceCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AvatarId",
                table: "Users",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundFeat");

            migrationBuilder.DropTable(
                name: "BackgroundItem");

            migrationBuilder.DropTable(
                name: "BackgroundSpell");

            migrationBuilder.DropTable(
                name: "CharacterAbilities");

            migrationBuilder.DropTable(
                name: "CharacterBackgrounds");

            migrationBuilder.DropTable(
                name: "CharacterClasses");

            migrationBuilder.DropTable(
                name: "CharacterFeats");

            migrationBuilder.DropTable(
                name: "Characteristics");

            migrationBuilder.DropTable(
                name: "CharacterItem");

            migrationBuilder.DropTable(
                name: "CharacterItem1");

            migrationBuilder.DropTable(
                name: "CharacterItem2");

            migrationBuilder.DropTable(
                name: "CharacterItems");

            migrationBuilder.DropTable(
                name: "CharacterRaces");

            migrationBuilder.DropTable(
                name: "CharacterSkills");

            migrationBuilder.DropTable(
                name: "CharacterSpells");

            migrationBuilder.DropTable(
                name: "ClassFeatureActions");

            migrationBuilder.DropTable(
                name: "ClassFeatureCreatures");

            migrationBuilder.DropTable(
                name: "ClassFeatureLevelScales");

            migrationBuilder.DropTable(
                name: "ClassFeatureOptions");

            migrationBuilder.DropTable(
                name: "ClassFeatureSpell");

            migrationBuilder.DropTable(
                name: "ClassFeatureSpells");

            migrationBuilder.DropTable(
                name: "ClassItem");

            migrationBuilder.DropTable(
                name: "ClassSpell");

            migrationBuilder.DropTable(
                name: "ClassSubclass");

            migrationBuilder.DropTable(
                name: "FeatActions");

            migrationBuilder.DropTable(
                name: "FeatOptions");

            migrationBuilder.DropTable(
                name: "FeatSpell");

            migrationBuilder.DropTable(
                name: "FeatSpells");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Modifiers");

            migrationBuilder.DropTable(
                name: "RaceTraitActions");

            migrationBuilder.DropTable(
                name: "RaceTraitCreatures");

            migrationBuilder.DropTable(
                name: "RaceTraitOptions");

            migrationBuilder.DropTable(
                name: "RaceTraitSpell");

            migrationBuilder.DropTable(
                name: "RaceTraitSpells");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SpellSubclass");

            migrationBuilder.DropTable(
                name: "StartItems");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ClassFeatures");

            migrationBuilder.DropTable(
                name: "Feats");

            migrationBuilder.DropTable(
                name: "Creatures");

            migrationBuilder.DropTable(
                name: "RaceTraits");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "StartItemChoices");

            migrationBuilder.DropTable(
                name: "Subclasses");

            migrationBuilder.DropTable(
                name: "Races");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "StoredFiles");
        }
    }
}
