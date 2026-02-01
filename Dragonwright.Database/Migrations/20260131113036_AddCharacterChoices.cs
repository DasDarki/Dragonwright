using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dragonwright.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumberOfUsesStatModifierAbility",
                table: "RaceTraitSpells",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AreaOfEffect",
                table: "RaceTraitActions",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeatId",
                table: "Modifiers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AbilityScoreIncrease",
                table: "Feats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AbilityScoreOptions",
                table: "Feats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Feats",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FeatLevel",
                table: "Feats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepeatable",
                table: "Feats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrerequisiteAbilityScore",
                table: "Feats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrerequisiteAbilityScoreMinimum",
                table: "Feats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrerequisiteDescription",
                table: "Feats",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PrerequisiteSpellcasting",
                table: "Feats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "NumberOfUsesStatModifierAbility",
                table: "ClassFeatureSpells",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AreaOfEffect",
                table: "ClassFeatureActions",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AlwaysPrepared",
                table: "CharacterSpells",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CastAtLevelOverride",
                table: "CharacterSpells",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrepared",
                table: "CharacterSpells",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxUses",
                table: "CharacterSpells",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetType",
                table: "CharacterSpells",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceClassId",
                table: "CharacterSpells",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpellSource",
                table: "CharacterSpells",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsesRemaining",
                table: "CharacterSpells",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "Characters",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Characters",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "DeathSaveFailures",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeathSaveSuccesses",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChosenSpells",
                table: "CharacterRaces",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenTraitOptions",
                table: "CharacterRaces",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenAbilityScoreIncrease",
                table: "CharacterFeats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChosenOptions",
                table: "CharacterFeats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenSpells",
                table: "CharacterFeats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenFeatureOptions",
                table: "CharacterClasses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenSkillProficiencies",
                table: "CharacterClasses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenSpells",
                table: "CharacterClasses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsStartingClass",
                table: "CharacterClasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PactSlotsUsed",
                table: "CharacterClasses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpellSlotsUsed",
                table: "CharacterClasses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenAbilityScoreIncreases",
                table: "CharacterBackgrounds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenCharacteristics",
                table: "CharacterBackgrounds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChosenLanguages",
                table: "CharacterBackgrounds",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateIndex(
                name: "IX_Modifiers_FeatId",
                table: "Modifiers",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpells_SourceClassId",
                table: "CharacterSpells",
                column: "SourceClassId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterSpells_Classes_SourceClassId",
                table: "CharacterSpells",
                column: "SourceClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Modifiers_Feats_FeatId",
                table: "Modifiers",
                column: "FeatId",
                principalTable: "Feats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterSpells_Classes_SourceClassId",
                table: "CharacterSpells");

            migrationBuilder.DropForeignKey(
                name: "FK_Modifiers_Feats_FeatId",
                table: "Modifiers");

            migrationBuilder.DropTable(
                name: "FeatActions");

            migrationBuilder.DropTable(
                name: "FeatOptions");

            migrationBuilder.DropTable(
                name: "FeatSpell");

            migrationBuilder.DropTable(
                name: "FeatSpells");

            migrationBuilder.DropIndex(
                name: "IX_Modifiers_FeatId",
                table: "Modifiers");

            migrationBuilder.DropIndex(
                name: "IX_CharacterSpells_SourceClassId",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "FeatId",
                table: "Modifiers");

            migrationBuilder.DropColumn(
                name: "AbilityScoreIncrease",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "AbilityScoreOptions",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "FeatLevel",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "IsRepeatable",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "PrerequisiteAbilityScore",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "PrerequisiteAbilityScoreMinimum",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "PrerequisiteDescription",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "PrerequisiteSpellcasting",
                table: "Feats");

            migrationBuilder.DropColumn(
                name: "AlwaysPrepared",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "CastAtLevelOverride",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "IsPrepared",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "MaxUses",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "ResetType",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "SourceClassId",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "SpellSource",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "UsesRemaining",
                table: "CharacterSpells");

            migrationBuilder.DropColumn(
                name: "DeathSaveFailures",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DeathSaveSuccesses",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ChosenSpells",
                table: "CharacterRaces");

            migrationBuilder.DropColumn(
                name: "ChosenTraitOptions",
                table: "CharacterRaces");

            migrationBuilder.DropColumn(
                name: "ChosenAbilityScoreIncrease",
                table: "CharacterFeats");

            migrationBuilder.DropColumn(
                name: "ChosenOptions",
                table: "CharacterFeats");

            migrationBuilder.DropColumn(
                name: "ChosenSpells",
                table: "CharacterFeats");

            migrationBuilder.DropColumn(
                name: "ChosenFeatureOptions",
                table: "CharacterClasses");

            migrationBuilder.DropColumn(
                name: "ChosenSkillProficiencies",
                table: "CharacterClasses");

            migrationBuilder.DropColumn(
                name: "ChosenSpells",
                table: "CharacterClasses");

            migrationBuilder.DropColumn(
                name: "IsStartingClass",
                table: "CharacterClasses");

            migrationBuilder.DropColumn(
                name: "PactSlotsUsed",
                table: "CharacterClasses");

            migrationBuilder.DropColumn(
                name: "SpellSlotsUsed",
                table: "CharacterClasses");

            migrationBuilder.DropColumn(
                name: "ChosenAbilityScoreIncreases",
                table: "CharacterBackgrounds");

            migrationBuilder.DropColumn(
                name: "ChosenCharacteristics",
                table: "CharacterBackgrounds");

            migrationBuilder.DropColumn(
                name: "ChosenLanguages",
                table: "CharacterBackgrounds");

            migrationBuilder.AlterColumn<int>(
                name: "NumberOfUsesStatModifierAbility",
                table: "RaceTraitSpells",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AreaOfEffect",
                table: "RaceTraitActions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NumberOfUsesStatModifierAbility",
                table: "ClassFeatureSpells",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AreaOfEffect",
                table: "ClassFeatureActions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Characters",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "Characters",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
