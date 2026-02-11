using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFenceDeviceCameraConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CameraConfig",
                table: "FenceDevices",
                type: "nvarchar(max)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameraConfig",
                table: "FenceDevices");
        }
    }
}
