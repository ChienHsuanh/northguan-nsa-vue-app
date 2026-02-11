using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class ParkingRecordService : IParkingRecordService
    {
        private readonly ApplicationDbContext _context;

        public ParkingRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<ParkingRecordListResponse>> GetRecordsListAsync(ParkingRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.ParkingRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

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
                    parkedNum = r.OccupiedSpaces,
                    time = r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    timestamp = ((DateTimeOffset)r.Time).ToUnixTimeSeconds()
                })
                .ToListAsync();

            // Get device and station info
            var devicesInfo = await _context.ParkingDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new { d.Serial, d.Name, d.NumberOfParking, StationName = d.Station!.Name })
                .AsNoTracking()
                .ToListAsync();

            var data = records.Select(r => new ParkingRecordListResponse
            {
                Id = r.id,
                DeviceSerial = r.deviceSerial,
                DeviceName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.Name ?? "未知裝置",
                StationName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.StationName ?? "未知分站",
                TotalSpaces = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.NumberOfParking ?? 0,
                ParkedNum = r.parkedNum,
                AvailableSpaces = (devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.NumberOfParking ?? 0) - r.parkedNum,
                OccupancyRate = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.NumberOfParking > 0 ?
                    (r.parkedNum / (double)devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial).NumberOfParking) * 100 : 0,
                Time = r.time,
                Timestamp = r.timestamp
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.Size);

            return new PagedResponse<ParkingRecordListResponse>
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

            return await _context.ParkingRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .CountAsync();
        }

        private record ParkingDeviceInfo(string Serial, string Name, int NumberOfParking, string StationName);

        public async IAsyncEnumerable<ParkingRecordListResponse> StreamAllParkingRecordsAsync(ParkingRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.ParkingRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply sorting
            query = parameters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.Time)
                : query.OrderByDescending(r => r.Time);

            // Fetch device and station info once
            var devicesInfo = await _context.ParkingDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new ParkingDeviceInfo(d.Serial, d.Name, d.NumberOfParking, d.Station!.Name))
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Serial!, d => d);

            await foreach (var record in query.AsAsyncEnumerable())
            {
                ParkingDeviceInfo? deviceInfo = null;
                devicesInfo.TryGetValue(record.DeviceSerial, out deviceInfo);

                var totalSpaces = deviceInfo?.NumberOfParking ?? 0;
                var parkedNum = record.OccupiedSpaces;
                var availableSpaces = totalSpaces - parkedNum;
                var occupancyRate = totalSpaces > 0 ? (parkedNum / (double)totalSpaces) * 100 : 0;

                yield return new ParkingRecordListResponse
                {
                    Id = record.Id,
                    DeviceSerial = record.DeviceSerial,
                    DeviceName = deviceInfo?.Name ?? "未知裝置",
                    StationName = deviceInfo?.StationName ?? "未知分站",
                    TotalSpaces = totalSpaces,
                    ParkedNum = parkedNum,
                    AvailableSpaces = availableSpaces,
                    OccupancyRate = Math.Round(occupancyRate, 2),
                    Time = record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)record.Time).ToUnixTimeSeconds()
                };
            }
        }

        private async Task<List<string>> GetAvailableDeviceSerialsAsync(List<int> availableStationIds, string keyword)
        {
            var query = _context.ParkingDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) && d.Station.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.Name.Contains(keyword) || d.Serial.Contains(keyword));
            }

            return await query.Select(d => d.Serial).ToListAsync();
        }
    }
}