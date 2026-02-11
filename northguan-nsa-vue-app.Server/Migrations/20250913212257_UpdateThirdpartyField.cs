using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateThirdpartyField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TravelTime",
                table: "TrafficRecords",
                type: "decimal(8,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalIn",
                table: "ParkingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalOut",
                table: "ParkingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TravelTime",
                table: "TrafficRecords");

            migrationBuilder.DropColumn(
                name: "TotalIn",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "TotalOut",
                table: "ParkingRecords");
        }
    }
}
