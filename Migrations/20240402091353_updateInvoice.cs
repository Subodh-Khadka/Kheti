using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class updateInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfData",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Invoices");

            migrationBuilder.AddColumn<byte[]>(
                name: "PdfData",
                table: "Invoices",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
