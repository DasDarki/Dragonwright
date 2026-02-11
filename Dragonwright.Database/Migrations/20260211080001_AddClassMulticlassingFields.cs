using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dragonwright.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddClassMulticlassingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MulticlassingRequirements",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MulticlassingRequirementsAlt",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StandardArray",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubclassSelectionLevel",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MulticlassingRequirements",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "MulticlassingRequirementsAlt",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "StandardArray",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "SubclassSelectionLevel",
                table: "Classes");
        }
    }
}
