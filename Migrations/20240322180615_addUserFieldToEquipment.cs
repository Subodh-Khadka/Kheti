using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class addUserFieldToEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RentalEquipment_ProductId",
                table: "RentalEquipment");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RentalEquipment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "khetiApplicationUserId",
                table: "RentalEquipment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalEquipment_khetiApplicationUserId",
                table: "RentalEquipment",
                column: "khetiApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalEquipment_ProductId",
                table: "RentalEquipment",
                column: "ProductId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalEquipment_AspNetUsers_khetiApplicationUserId",
                table: "RentalEquipment",
                column: "khetiApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalEquipment_AspNetUsers_khetiApplicationUserId",
                table: "RentalEquipment");

            migrationBuilder.DropIndex(
                name: "IX_RentalEquipment_khetiApplicationUserId",
                table: "RentalEquipment");

            migrationBuilder.DropIndex(
                name: "IX_RentalEquipment_ProductId",
                table: "RentalEquipment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RentalEquipment");

            migrationBuilder.DropColumn(
                name: "khetiApplicationUserId",
                table: "RentalEquipment");

            migrationBuilder.CreateIndex(
                name: "IX_RentalEquipment_ProductId",
                table: "RentalEquipment",
                column: "ProductId");
        }
    }
}
