using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class AddExpertProfileToDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpertProfile_AspNetUsers_UserId",
                table: "ExpertProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExpertProfile",
                table: "ExpertProfile");

            migrationBuilder.RenameTable(
                name: "ExpertProfile",
                newName: "ExpertProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_ExpertProfile_UserId",
                table: "ExpertProfiles",
                newName: "IX_ExpertProfiles_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExpertProfiles",
                table: "ExpertProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpertProfiles_AspNetUsers_UserId",
                table: "ExpertProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpertProfiles_AspNetUsers_UserId",
                table: "ExpertProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExpertProfiles",
                table: "ExpertProfiles");

            migrationBuilder.RenameTable(
                name: "ExpertProfiles",
                newName: "ExpertProfile");

            migrationBuilder.RenameIndex(
                name: "IX_ExpertProfiles_UserId",
                table: "ExpertProfile",
                newName: "IX_ExpertProfile_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExpertProfile",
                table: "ExpertProfile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpertProfile_AspNetUsers_UserId",
                table: "ExpertProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
