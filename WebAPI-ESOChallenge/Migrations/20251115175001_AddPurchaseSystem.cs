using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_ESOChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VBucks",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserCosmetics",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CosmeticId = table.Column<string>(type: "text", nullable: false),
                    PurchasePrice = table.Column<int>(type: "integer", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCosmetics", x => new { x.UserId, x.CosmeticId });
                    table.ForeignKey(
                        name: "FK_UserCosmetics_Cosmetics_CosmeticId",
                        column: x => x.CosmeticId,
                        principalTable: "Cosmetics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCosmetics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCosmetics_CosmeticId",
                table: "UserCosmetics",
                column: "CosmeticId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCosmetics_IsRefunded",
                table: "UserCosmetics",
                column: "IsRefunded");

            migrationBuilder.CreateIndex(
                name: "IX_UserCosmetics_PurchasedAt",
                table: "UserCosmetics",
                column: "PurchasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCosmetics_UserId",
                table: "UserCosmetics",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCosmetics");

            migrationBuilder.DropColumn(
                name: "VBucks",
                table: "Users");
        }
    }
}
