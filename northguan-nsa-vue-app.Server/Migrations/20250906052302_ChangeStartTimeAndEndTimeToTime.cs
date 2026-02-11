using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStartTimeAndEndTimeToTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "TrafficRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ParkingRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "HighResolutionRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "FenceRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ECvpTouristRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "CrowdRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "CrowdRecordLatests");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "AdmissionRecords");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "TrafficRecords",
                newName: "Time");

            migrationBuilder.RenameIndex(
                name: "IX_TrafficRecords_StartTime",
                table: "TrafficRecords",
                newName: "IX_TrafficRecords_Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "ParkingRecords",
                newName: "Time");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingRecords_StartTime",
                table: "ParkingRecords",
                newName: "IX_ParkingRecords_Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "HighResolutionRecords",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "FenceRecords",
                newName: "Time");

            migrationBuilder.RenameIndex(
                name: "IX_FenceRecords_StartTime",
                table: "FenceRecords",
                newName: "IX_FenceRecords_Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "ECvpTouristRecords",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "CrowdRecords",
                newName: "Time");

            migrationBuilder.RenameIndex(
                name: "IX_CrowdRecords_StartTime",
                table: "CrowdRecords",
                newName: "IX_CrowdRecords_Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "CrowdRecordLatests",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "AdmissionRecords",
                newName: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Time",
                table: "TrafficRecords",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_TrafficRecords_Time",
                table: "TrafficRecords",
                newName: "IX_TrafficRecords_StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "ParkingRecords",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingRecords_Time",
                table: "ParkingRecords",
                newName: "IX_ParkingRecords_StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "HighResolutionRecords",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "FenceRecords",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_FenceRecords_Time",
                table: "FenceRecords",
                newName: "IX_FenceRecords_StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "ECvpTouristRecords",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "CrowdRecords",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_CrowdRecords_Time",
                table: "CrowdRecords",
                newName: "IX_CrowdRecords_StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "CrowdRecordLatests",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "AdmissionRecords",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "TrafficRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ParkingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "HighResolutionRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "FenceRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ECvpTouristRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "CrowdRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "CrowdRecordLatests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "AdmissionRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
