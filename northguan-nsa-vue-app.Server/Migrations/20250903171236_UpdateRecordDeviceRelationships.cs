using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecordDeviceRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_TrafficDevices_Serial",
                table: "TrafficDevices",
                column: "Serial");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParkingDevices_Serial",
                table: "ParkingDevices",
                column: "Serial");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_FenceDevices_Serial",
                table: "FenceDevices",
                column: "Serial");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_CrowdDevices_Serial",
                table: "CrowdDevices",
                column: "Serial");

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

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TrafficDevices_Serial",
                table: "TrafficDevices");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParkingDevices_Serial",
                table: "ParkingDevices");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_FenceDevices_Serial",
                table: "FenceDevices");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_CrowdDevices_Serial",
                table: "CrowdDevices");
        }
    }
}
