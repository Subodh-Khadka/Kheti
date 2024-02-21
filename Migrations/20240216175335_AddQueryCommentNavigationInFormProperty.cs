using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryCommentNavigationInFormProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QueryFormId1",
                table: "QueryComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryComments_QueryFormId1",
                table: "QueryComments",
                column: "QueryFormId1",
                unique: true,
                filter: "[QueryFormId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_QueryComments_QueryForms_QueryFormId1",
                table: "QueryComments",
                column: "QueryFormId1",
                principalTable: "QueryForms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueryComments_QueryForms_QueryFormId1",
                table: "QueryComments");

            migrationBuilder.DropIndex(
                name: "IX_QueryComments_QueryFormId1",
                table: "QueryComments");

            migrationBuilder.DropColumn(
                name: "QueryFormId1",
                table: "QueryComments");
        }
    }
}
