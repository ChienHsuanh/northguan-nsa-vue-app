using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiUrl",
                table: "FenceDevices");

            migrationBuilder.DropColumn(
                name: "NumberOfParking",
                table: "FenceDevices");

            migrationBuilder.RenameColumn(
                name: "ObservingTime",
                table: "FenceDevices",
                newName: "ObservingTimeStart");

            migrationBuilder.AddColumn<string>(
                name: "ObservingTimeEnd",
                table: "FenceDevices",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObservingTimeEnd",
                table: "FenceDevices");

            migrationBuilder.RenameColumn(
                name: "ObservingTimeStart",
                table: "FenceDevices",
                newName: "ObservingTime");

            migrationBuilder.AddColumn<string>(
                name: "ApiUrl",
                table: "FenceDevices",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfParking",
                table: "FenceDevices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
