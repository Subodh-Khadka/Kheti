using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class addNavigationForProductComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductCommentId1",
                table: "ProductReplies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "ProductReplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId1",
                table: "ProductComments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReplies_ProductCommentId1",
                table: "ProductReplies",
                column: "ProductCommentId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReplies_ProductId",
                table: "ProductReplies",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_ProductId1",
                table: "ProductComments",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_Products_ProductId1",
                table: "ProductComments",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReplies_ProductComments_ProductCommentId1",
                table: "ProductReplies",
                column: "ProductCommentId1",
                principalTable: "ProductComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReplies_Products_ProductId",
                table: "ProductReplies",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_Products_ProductId1",
                table: "ProductComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReplies_ProductComments_ProductCommentId1",
                table: "ProductReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReplies_Products_ProductId",
                table: "ProductReplies");

            migrationBuilder.DropIndex(
                name: "IX_ProductReplies_ProductCommentId1",
                table: "ProductReplies");

            migrationBuilder.DropIndex(
                name: "IX_ProductReplies_ProductId",
                table: "ProductReplies");

            migrationBuilder.DropIndex(
                name: "IX_ProductComments_ProductId1",
                table: "ProductComments");

            migrationBuilder.DropColumn(
                name: "ProductCommentId1",
                table: "ProductReplies");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductReplies");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "ProductComments");
        }
    }
}
