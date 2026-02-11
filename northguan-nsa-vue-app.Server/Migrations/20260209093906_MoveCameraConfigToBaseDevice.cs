using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class MoveCameraConfigToBaseDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CameraConfig",
                table: "TrafficDevices",
                type: "nvarchar(max)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CameraConfig",
                table: "ParkingDevices",
                type: "nvarchar(max)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CameraConfig",
                table: "HighResolutionDevices",
                type: "nvarchar(max)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CameraConfig",
                table: "CrowdDevices",
                type: "nvarchar(max)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameraConfig",
                table: "TrafficDevices");

            migrationBuilder.DropColumn(
                name: "CameraConfig",
                table: "ParkingDevices");

            migrationBuilder.DropColumn(
                name: "CameraConfig",
                table: "HighResolutionDevices");

            migrationBuilder.DropColumn(
                name: "CameraConfig",
                table: "CrowdDevices");
        }
    }
}
