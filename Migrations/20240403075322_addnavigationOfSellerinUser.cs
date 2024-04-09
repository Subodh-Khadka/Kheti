using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class addnavigationOfSellerinUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles",
                column: "UserId");
        }
    }
}
