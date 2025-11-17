using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_ESOChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AddBundleIdToUserCosmetics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BundleId",
                table: "UserCosmetics",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCosmetics_BundleId",
                table: "UserCosmetics",
                column: "BundleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserCosmetics_BundleId",
                table: "UserCosmetics");

            migrationBuilder.DropColumn(
                name: "BundleId",
                table: "UserCosmetics");
        }
    }
}
