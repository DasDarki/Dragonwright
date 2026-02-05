using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dragonwright.Database.Migrations
{
    /// <inheritdoc />
    public partial class ExtendLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Script",
                table: "Languages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Languages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TypicalSpeakers",
                table: "Languages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Script",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "TypicalSpeakers",
                table: "Languages");
        }
    }
}
