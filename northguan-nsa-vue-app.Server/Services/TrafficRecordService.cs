using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class TrafficRecordService : ITrafficRecordService
    {
        private readonly ApplicationDbContext _context;

        public TrafficRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<TrafficRecordListResponse>> GetRecordsListAsync(TrafficRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.TrafficRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply vehicle count filters
            if (parameters.MinVehicleCount.HasValue)
                query = query.Where(r => r.VehicleCount >= parameters.MinVehicleCount.Value);
            if (parameters.MaxVehicleCount.HasValue)
                query = query.Where(r => r.VehicleCount <= parameters.MaxVehicleCount.Value);

            // Apply speed filters
            if (parameters.MinAverageSpeed.HasValue)
                query = query.Where(r => r.AverageSpeed >= parameters.MinAverageSpeed.Value);
            if (parameters.MaxAverageSpeed.HasValue)
                query = query.Where(r => r.AverageSpeed <= parameters.MaxAverageSpeed.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = parameters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.Time)
                : query.OrderByDescending(r => r.Time);

            var records = await query
                .Skip((parameters.Page - 1) * parameters.Size)
                .Take(parameters.Size)
                .Select(r => new
                {
                    id = r.Id,
                    deviceSerial = r.DeviceSerial,
                    vehicleCount = r.VehicleCount,
                    averageSpeed = r.AverageSpeed,
                    time = r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    timestamp = ((DateTimeOffset)r.Time).ToUnixTimeSeconds()
                })
                .ToListAsync();

            // Get device and station info
            var devicesInfo = await _context.TrafficDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new { d.Serial, d.Name, d.City, d.ETagNumber, d.SpeedLimit, StationName = d.Station!.Name })
                .AsNoTracking()
                .ToListAsync();

            var data = records.Select(r => new TrafficRecordListResponse
            {
                Id = r.id,
                DeviceSerial = r.deviceSerial,
                DeviceName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.Name ?? "未知裝置",
                StationName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.StationName ?? "未知分站",
                City = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.City ?? "",
                ETagNumber = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.ETagNumber ?? "",
                SpeedLimit = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.SpeedLimit ?? 0,
                VehicleCount = r.vehicleCount,
                AverageSpeed = r.averageSpeed,
                SpeedStatus = GetSpeedStatus(r.averageSpeed, devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.SpeedLimit ?? 0),
                Time = r.time,
                Timestamp = r.timestamp
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.Size);

            return new PagedResponse<TrafficRecordListResponse>
            {
                Data = data,
                TotalCount = totalCount,
                Page = parameters.Page,
                Size = parameters.Size,
                TotalPages = totalPages,
                HasNextPage = parameters.Page < totalPages,
                HasPreviousPage = parameters.Page > 1,
                Success = true
            };
        }

        public async Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(availableStationIds, keyword);

            return await _context.TrafficRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .CountAsync();
        }

        private record TrafficDeviceInfo(string Serial, string Name, string City, string ETagNumber, int SpeedLimit, string StationName);

        public async IAsyncEnumerable<TrafficRecordListResponse> StreamAllTrafficRecordsAsync(TrafficRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.TrafficRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply vehicle count filters
            if (parameters.MinVehicleCount.HasValue)
                query = query.Where(r => r.VehicleCount >= parameters.MinVehicleCount.Value);
            if (parameters.MaxVehicleCount.HasValue)
                query = query.Where(r => r.VehicleCount <= parameters.MaxVehicleCount.Value);

            // Apply speed filters
            if (parameters.MinAverageSpeed.HasValue)
                query = query.Where(r => r.AverageSpeed >= parameters.MinAverageSpeed.Value);
            if (parameters.MaxAverageSpeed.HasValue)
                query = query.Where(r => r.AverageSpeed <= parameters.MaxAverageSpeed.Value);

            // Apply sorting
            query = parameters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.Time)
                : query.OrderByDescending(r => r.Time);

            // Fetch device and station info once
            var devicesInfo = await _context.TrafficDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new TrafficDeviceInfo(d.Serial, d.Name, d.City, d.ETagNumber, d.SpeedLimit, d.Station!.Name))
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Serial!, d => d);

            await foreach (var record in query.AsAsyncEnumerable())
            {
                TrafficDeviceInfo? deviceInfo = null;
                devicesInfo.TryGetValue(record.DeviceSerial, out deviceInfo);

                var speedStatus = GetSpeedStatus(record.AverageSpeed, deviceInfo?.SpeedLimit ?? 0);

                yield return new TrafficRecordListResponse
                {
                    Id = record.Id,
                    DeviceSerial = record.DeviceSerial,
                    DeviceName = deviceInfo?.Name ?? "未知裝置",
                    StationName = deviceInfo?.StationName ?? "未知分站",
                    City = deviceInfo?.City ?? "",
                    ETagNumber = deviceInfo?.ETagNumber ?? "",
                    SpeedLimit = deviceInfo?.SpeedLimit ?? 0,
                    VehicleCount = record.VehicleCount,
                    AverageSpeed = record.AverageSpeed,
                    SpeedStatus = speedStatus,
                    Time = record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)record.Time).ToUnixTimeSeconds()
                };
            }
        }

        private async Task<List<string>> GetAvailableDeviceSerialsAsync(List<int> availableStationIds, string keyword)
        {
            var query = _context.TrafficDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) && d.Station.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.Name.Contains(keyword) || d.Serial.Contains(keyword));
            }

            return await query.Select(d => d.Serial).ToListAsync();
        }

        private static string GetSpeedStatus(decimal averageSpeed, int speedLimit)
        {
            if (speedLimit == 0) return "無速限";

            var speedRatio = (double)averageSpeed / speedLimit;
            return speedRatio switch
            {
                > 1.2 => "超速嚴重",
                > 1.0 => "輕微超速",
                > 0.8 => "正常",
                > 0.5 => "緩慢",
                _ => "壅塞"
            };
        }
    }
}