using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryCommentAndReplyToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueryComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QueryFormId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueryComments_QueryForms_QueryFormId",
                        column: x => x.QueryFormId,
                        principalTable: "QueryForms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QueryReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReplyText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QueryCommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QueryCommentId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryReplies_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueryReplies_QueryComments_QueryCommentId",
                        column: x => x.QueryCommentId,
                        principalTable: "QueryComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QueryReplies_QueryComments_QueryCommentId1",
                        column: x => x.QueryCommentId1,
                        principalTable: "QueryComments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueryComments_QueryFormId",
                table: "QueryComments",
                column: "QueryFormId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryComments_UserId",
                table: "QueryComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryReplies_QueryCommentId",
                table: "QueryReplies",
                column: "QueryCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryReplies_QueryCommentId1",
                table: "QueryReplies",
                column: "QueryCommentId1",
                unique: true,
                filter: "[QueryCommentId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QueryReplies_UserId",
                table: "QueryReplies",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueryReplies");

            migrationBuilder.DropTable(
                name: "QueryComments");
        }
    }
}
