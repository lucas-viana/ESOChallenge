using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_ESOChallenge.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsAvailableToIsInShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "Cosmetics",
                newName: "IsInShop");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInShop",
                table: "Cosmetics",
                newName: "IsAvailable");
        }
    }
}
