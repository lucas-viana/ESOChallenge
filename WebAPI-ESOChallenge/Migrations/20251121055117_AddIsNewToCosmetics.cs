using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_ESOChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNewToCosmetics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Cosmetics",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Cosmetics");
        }
    }
}
