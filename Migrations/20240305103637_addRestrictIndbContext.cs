using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class addRestrictIndbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Products_ProductId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_Products_ProductId",
                table: "ProductComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReplies_ProductComments_ProductCommentId",
                table: "ProductReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryComments_QueryForms_QueryFormId",
                table: "QueryComments");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryReplies_QueryComments_QueryCommentId",
                table: "QueryReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Products_ProductId",
                table: "Favorites",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_Products_ProductId",
                table: "ProductComments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReplies_ProductComments_ProductCommentId",
                table: "ProductReplies",
                column: "ProductCommentId",
                principalTable: "ProductComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QueryComments_QueryForms_QueryFormId",
                table: "QueryComments",
                column: "QueryFormId",
                principalTable: "QueryForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QueryReplies_QueryComments_QueryCommentId",
                table: "QueryReplies",
                column: "QueryCommentId",
                principalTable: "QueryComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Products_ProductId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_Products_ProductId",
                table: "ProductComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReplies_ProductComments_ProductCommentId",
                table: "ProductReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryComments_QueryForms_QueryFormId",
                table: "QueryComments");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryReplies_QueryComments_QueryCommentId",
                table: "QueryReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Products_ProductId",
                table: "Favorites",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_Products_ProductId",
                table: "ProductComments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReplies_ProductComments_ProductCommentId",
                table: "ProductReplies",
                column: "ProductCommentId",
                principalTable: "ProductComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueryComments_QueryForms_QueryFormId",
                table: "QueryComments",
                column: "QueryFormId",
                principalTable: "QueryForms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueryReplies_QueryComments_QueryCommentId",
                table: "QueryReplies",
                column: "QueryCommentId",
                principalTable: "QueryComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_UserId",
                table: "ShoppingCarts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
