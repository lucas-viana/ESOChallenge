using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_ESOChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AddBundleSupportColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBundle",
                table: "Cosmetics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ContainedItemIdsJson",
                table: "Cosmetics",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBundle",
                table: "Cosmetics");

            migrationBuilder.DropColumn(
                name: "ContainedItemIdsJson",
                table: "Cosmetics");
        }
    }
}
