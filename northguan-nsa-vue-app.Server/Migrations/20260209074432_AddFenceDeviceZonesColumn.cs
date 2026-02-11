using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFenceDeviceZonesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrowdRecords_CrowdDevices_DeviceSerial",
                table: "CrowdRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FenceRecords_FenceDevices_DeviceSerial",
                table: "FenceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingRecords_ParkingDevices_DeviceSerial",
                table: "ParkingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_TrafficRecords_TrafficDevices_DeviceSerial",
                table: "TrafficRecords");

            migrationBuilder.AddColumn<string>(
                name: "Zones",
                table: "FenceDevices",
                type: "nvarchar(max)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CrowdRecords_CrowdDevices_DeviceSerial",
                table: "CrowdRecords",
                column: "DeviceSerial",
                principalTable: "CrowdDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FenceRecords_FenceDevices_DeviceSerial",
                table: "FenceRecords",
                column: "DeviceSerial",
                principalTable: "FenceDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingRecords_ParkingDevices_DeviceSerial",
                table: "ParkingRecords",
                column: "DeviceSerial",
                principalTable: "ParkingDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrafficRecords_TrafficDevices_DeviceSerial",
                table: "TrafficRecords",
                column: "DeviceSerial",
                principalTable: "TrafficDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrowdRecords_CrowdDevices_DeviceSerial",
                table: "CrowdRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FenceRecords_FenceDevices_DeviceSerial",
                table: "FenceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingRecords_ParkingDevices_DeviceSerial",
                table: "ParkingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_TrafficRecords_TrafficDevices_DeviceSerial",
                table: "TrafficRecords");

            migrationBuilder.DropColumn(
                name: "Zones",
                table: "FenceDevices");

            migrationBuilder.AddForeignKey(
                name: "FK_CrowdRecords_CrowdDevices_DeviceSerial",
                table: "CrowdRecords",
                column: "DeviceSerial",
                principalTable: "CrowdDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FenceRecords_FenceDevices_DeviceSerial",
                table: "FenceRecords",
                column: "DeviceSerial",
                principalTable: "FenceDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingRecords_ParkingDevices_DeviceSerial",
                table: "ParkingRecords",
                column: "DeviceSerial",
                principalTable: "ParkingDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrafficRecords_TrafficDevices_DeviceSerial",
                table: "TrafficRecords",
                column: "DeviceSerial",
                principalTable: "TrafficDevices",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
