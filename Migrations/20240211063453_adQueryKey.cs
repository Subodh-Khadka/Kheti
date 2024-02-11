using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class adQueryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QueryForms",
                table: "QueryForms");

            migrationBuilder.DropColumn(
                name: "QueryFormId",
                table: "QueryForms");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "QueryForms",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueryForms",
                table: "QueryForms",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QueryForms",
                table: "QueryForms");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "QueryForms");

            migrationBuilder.AddColumn<string>(
                name: "QueryFormId",
                table: "QueryForms",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueryForms",
                table: "QueryForms",
                column: "QueryFormId");
        }
    }
}
