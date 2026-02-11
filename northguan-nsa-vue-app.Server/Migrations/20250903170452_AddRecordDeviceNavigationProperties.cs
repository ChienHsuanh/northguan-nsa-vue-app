using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordDeviceNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NumberOfParking",
                table: "ParkingDevices",
                type: "int",
                nullable: false,
                comment: "停車位數",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Area",
                table: "CrowdDevices",
                type: "int",
                nullable: false,
                comment: "面積(平方公尺)",
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NumberOfParking",
                table: "ParkingDevices",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "停車位數");

            migrationBuilder.AlterColumn<int>(
                name: "Area",
                table: "CrowdDevices",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "面積(平方公尺)");
        }
    }
}
