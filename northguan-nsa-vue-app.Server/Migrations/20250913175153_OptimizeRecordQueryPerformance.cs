using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeRecordQueryPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrafficDevices_StationId",
                table: "TrafficDevices");

            migrationBuilder.DropIndex(
                name: "IX_ParkingDevices_StationId",
                table: "ParkingDevices");

            migrationBuilder.DropIndex(
                name: "IX_HighResolutionDevices_StationId",
                table: "HighResolutionDevices");

            migrationBuilder.DropIndex(
                name: "IX_FenceDevices_StationId",
                table: "FenceDevices");

            migrationBuilder.DropIndex(
                name: "IX_CrowdDevices_StationId",
                table: "CrowdDevices");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficRecords_AverageSpeed_Time_Composite",
                table: "TrafficRecords",
                columns: new[] { "AverageSpeed", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficRecords_DeviceSerial_Time_Composite",
                table: "TrafficRecords",
                columns: new[] { "DeviceSerial", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficRecords_Time_DeviceSerial_Composite",
                table: "TrafficRecords",
                columns: new[] { "Time", "DeviceSerial" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficRecords_VehicleCount_Time_Composite",
                table: "TrafficRecords",
                columns: new[] { "VehicleCount", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficDevices_Name_Serial_Composite",
                table: "TrafficDevices",
                columns: new[] { "Name", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficDevices_StationId_DeletedAt_Composite",
                table: "TrafficDevices",
                columns: new[] { "StationId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Stations_DeletedAt_Performance",
                table: "Stations",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_DeviceSerial_Time_Composite",
                table: "ParkingRecords",
                columns: new[] { "DeviceSerial", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_Time_DeviceSerial_Composite",
                table: "ParkingRecords",
                columns: new[] { "Time", "DeviceSerial" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingDevices_Name_Serial_Composite",
                table: "ParkingDevices",
                columns: new[] { "Name", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingDevices_StationId_DeletedAt_Composite",
                table: "ParkingDevices",
                columns: new[] { "StationId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HighResolutionDevices_Name_Serial_Composite",
                table: "HighResolutionDevices",
                columns: new[] { "Name", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_HighResolutionDevices_StationId_DeletedAt_Composite",
                table: "HighResolutionDevices",
                columns: new[] { "StationId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FenceRecords_DeviceSerial_Time_Composite",
                table: "FenceRecords",
                columns: new[] { "DeviceSerial", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FenceRecords_EventType_Time_Composite",
                table: "FenceRecords",
                columns: new[] { "EventType", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FenceRecords_Time_DeviceSerial_Composite",
                table: "FenceRecords",
                columns: new[] { "Time", "DeviceSerial" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_FenceDevices_Name_Serial_Composite",
                table: "FenceDevices",
                columns: new[] { "Name", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_FenceDevices_StationId_DeletedAt_Composite",
                table: "FenceDevices",
                columns: new[] { "StationId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CrowdRecords_Count_Time_Composite",
                table: "CrowdRecords",
                columns: new[] { "Count", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_CrowdRecords_DeviceSerial_Time_Composite",
                table: "CrowdRecords",
                columns: new[] { "DeviceSerial", "Time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_CrowdRecords_Time_DeviceSerial_Composite",
                table: "CrowdRecords",
                columns: new[] { "Time", "DeviceSerial" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_CrowdDevices_Name_Serial_Composite",
                table: "CrowdDevices",
                columns: new[] { "Name", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_CrowdDevices_StationId_DeletedAt_Composite",
                table: "CrowdDevices",
                columns: new[] { "StationId", "DeletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrafficRecords_AverageSpeed_Time_Composite",
                table: "TrafficRecords");

            migrationBuilder.DropIndex(
                name: "IX_TrafficRecords_DeviceSerial_Time_Composite",
                table: "TrafficRecords");

            migrationBuilder.DropIndex(
                name: "IX_TrafficRecords_Time_DeviceSerial_Composite",
                table: "TrafficRecords");

            migrationBuilder.DropIndex(
                name: "IX_TrafficRecords_VehicleCount_Time_Composite",
                table: "TrafficRecords");

            migrationBuilder.DropIndex(
                name: "IX_TrafficDevices_Name_Serial_Composite",
                table: "TrafficDevices");

            migrationBuilder.DropIndex(
                name: "IX_TrafficDevices_StationId_DeletedAt_Composite",
                table: "TrafficDevices");

            migrationBuilder.DropIndex(
                name: "IX_Stations_DeletedAt_Performance",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_ParkingRecords_DeviceSerial_Time_Composite",
                table: "ParkingRecords");

            migrationBuilder.DropIndex(
                name: "IX_ParkingRecords_Time_DeviceSerial_Composite",
                table: "ParkingRecords");

            migrationBuilder.DropIndex(
                name: "IX_ParkingDevices_Name_Serial_Composite",
                table: "ParkingDevices");

            migrationBuilder.DropIndex(
                name: "IX_ParkingDevices_StationId_DeletedAt_Composite",
                table: "ParkingDevices");

            migrationBuilder.DropIndex(
                name: "IX_HighResolutionDevices_Name_Serial_Composite",
                table: "HighResolutionDevices");

            migrationBuilder.DropIndex(
                name: "IX_HighResolutionDevices_StationId_DeletedAt_Composite",
                table: "HighResolutionDevices");

            migrationBuilder.DropIndex(
                name: "IX_FenceRecords_DeviceSerial_Time_Composite",
                table: "FenceRecords");

            migrationBuilder.DropIndex(
                name: "IX_FenceRecords_EventType_Time_Composite",
                table: "FenceRecords");

            migrationBuilder.DropIndex(
                name: "IX_FenceRecords_Time_DeviceSerial_Composite",
                table: "FenceRecords");

            migrationBuilder.DropIndex(
                name: "IX_FenceDevices_Name_Serial_Composite",
                table: "FenceDevices");

            migrationBuilder.DropIndex(
                name: "IX_FenceDevices_StationId_DeletedAt_Composite",
                table: "FenceDevices");

            migrationBuilder.DropIndex(
                name: "IX_CrowdRecords_Count_Time_Composite",
                table: "CrowdRecords");

            migrationBuilder.DropIndex(
                name: "IX_CrowdRecords_DeviceSerial_Time_Composite",
                table: "CrowdRecords");

            migrationBuilder.DropIndex(
                name: "IX_CrowdRecords_Time_DeviceSerial_Composite",
                table: "CrowdRecords");

            migrationBuilder.DropIndex(
                name: "IX_CrowdDevices_Name_Serial_Composite",
                table: "CrowdDevices");

            migrationBuilder.DropIndex(
                name: "IX_CrowdDevices_StationId_DeletedAt_Composite",
                table: "CrowdDevices");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficDevices_StationId",
                table: "TrafficDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingDevices_StationId",
                table: "ParkingDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_HighResolutionDevices_StationId",
                table: "HighResolutionDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_FenceDevices_StationId",
                table: "FenceDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_CrowdDevices_StationId",
                table: "CrowdDevices",
                column: "StationId");
        }
    }
}
