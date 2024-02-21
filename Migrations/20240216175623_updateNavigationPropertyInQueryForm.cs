using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class updateNavigationPropertyInQueryForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueryComments_QueryFormId1",
                table: "QueryComments");

            migrationBuilder.AddColumn<int>(
                name: "QueryFormId",
                table: "QueryReplies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryReplies_QueryFormId",
                table: "QueryReplies",
                column: "QueryFormId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryComments_QueryFormId1",
                table: "QueryComments",
                column: "QueryFormId1");

            migrationBuilder.AddForeignKey(
                name: "FK_QueryReplies_QueryForms_QueryFormId",
                table: "QueryReplies",
                column: "QueryFormId",
                principalTable: "QueryForms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueryReplies_QueryForms_QueryFormId",
                table: "QueryReplies");

            migrationBuilder.DropIndex(
                name: "IX_QueryReplies_QueryFormId",
                table: "QueryReplies");

            migrationBuilder.DropIndex(
                name: "IX_QueryComments_QueryFormId1",
                table: "QueryComments");

            migrationBuilder.DropColumn(
                name: "QueryFormId",
                table: "QueryReplies");

            migrationBuilder.CreateIndex(
                name: "IX_QueryComments_QueryFormId1",
                table: "QueryComments",
                column: "QueryFormId1",
                unique: true,
                filter: "[QueryFormId1] IS NOT NULL");
        }
    }
}
