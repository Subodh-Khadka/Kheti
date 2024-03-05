using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class addRestrictInSCartInDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
