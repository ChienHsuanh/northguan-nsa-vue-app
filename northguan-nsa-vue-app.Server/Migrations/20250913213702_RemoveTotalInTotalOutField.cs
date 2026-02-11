using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotalInTotalOutField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalIn",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "TotalOut",
                table: "ParkingRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
