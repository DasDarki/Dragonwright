using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dragonwright.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatUsagesAndItemCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargesUsed",
                table: "CharacterItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxCharges",
                table: "CharacterItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FeatActionUsages",
                table: "CharacterFeats",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargesUsed",
                table: "CharacterItems");

            migrationBuilder.DropColumn(
                name: "MaxCharges",
                table: "CharacterItems");

            migrationBuilder.DropColumn(
                name: "FeatActionUsages",
                table: "CharacterFeats");
        }
    }
}
