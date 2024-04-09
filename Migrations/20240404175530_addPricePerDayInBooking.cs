using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kheti.Migrations
{
    /// <inheritdoc />
    public partial class addPricePerDayInBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "Bookings");
        }
    }
}
