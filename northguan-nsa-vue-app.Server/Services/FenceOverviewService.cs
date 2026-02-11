using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class FenceOverviewService : IFenceOverviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public FenceOverviewService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<FenceRecordHistoryResponse> GetRecentRecordHistoryAsync(int stationId, int timeRange, List<int> availableStationIds)
        {
            try
            {
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.FenceRecords
                            join device in _context.FenceDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            orderby record.Time descending
                            select new { record, device, station };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(r => new FenceHistoryData
                {
                    Time = r.record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)r.record.Time).ToUnixTimeMilliseconds(),
                    EventType = (int)r.record.EventType,
                    Event = r.record.EventType.GetDescription(),
                    StationName = r.station.Name,
                    DeviceName = r.device.Name
                }).ToList();

                return new FenceRecordHistoryResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new FenceRecordHistoryResponse
                {
                    Success = false,
                    Message = $"Error retrieving fence record history: {ex.Message}"
                };
            }
        }

        public async Task<FenceRecentRecordDetailResponse> GetRecentRecordAsync(int stationId, int timeRange, int limit, List<int> availableStationIds)
        {
            try
            {
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.FenceRecords
                            join device in _context.FenceDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && device.DeletedAt == null
                            select new { record, device, station };

                if (stationId > 0)
                {
                    query = query.Where(x => x.station.Id == stationId);
                }

                var results = await query
                    .OrderByDescending(x => x.record.Time)
                    .Take(limit)
                    .AsNoTracking()
                    .ToListAsync();

                var data = results.Select(r => new FenceRecentRecordDetail
                {
                    Id = r.record.Id,
                    StationId = r.station.Id,
                    DeviceName = r.device.Name,
                    Station = r.station.Name,
                    EventType = (int)r.record.EventType,
                    Event = r.record.EventType.GetDescription(),
                    Title = GenerateFenceEventMessage(r.device.Name, r.station.Name, r.record),
                    ImageUrl = !string.IsNullOrEmpty(r.record.Photo)
                        ? _fileService.GetUploadFileUrl("fence-record", r.record.Photo)
                        : "",
                    Time = r.record.Time.ToString("yyyy/MM/dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)r.record.Time).ToUnixTimeMilliseconds()
                }).ToList();

                return new FenceRecentRecordDetailResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new FenceRecentRecordDetailResponse
                {
                    Success = false,
                    Message = $"Error retrieving fence recent records: {ex.Message}"
                };
            }
        }

        public async Task<FenceLatestRecordResponse> GetLatestRecordAsync(int stationId, List<int> availableStationIds)
        {
            try
            {
                var query = from record in _context.FenceRecords
                            join device in _context.FenceDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            group new { record, device, station } by new { device.Id, DeviceName = device.Name, StationName = station.Name } into g
                            select new
                            {
                                DeviceName = g.Key.DeviceName,
                                StationName = g.Key.StationName,
                                LatestRecord = g.OrderByDescending(r => r.record.Time).First()
                            };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(g => new FenceLatestData
                {
                    Id = g.LatestRecord.record.Id,
                    StationId = g.LatestRecord.station.Id,
                    DeviceName = g.LatestRecord.device.Name,
                    StationName = g.LatestRecord.station.Name,
                    EventType = (int)g.LatestRecord.record.EventType,
                    Event = g.LatestRecord.record.EventType.GetDescription(),
                    ImageUrl = g.LatestRecord.record.Photo ?? "",
                    Time = g.LatestRecord.record.Time.ToString("yyyy/MM/dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)g.LatestRecord.record.Time).ToUnixTimeSeconds()
                }).ToList();

                return new FenceLatestRecordResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new FenceLatestRecordResponse
                {
                    Success = false,
                    Message = $"Error retrieving fence latest records: {ex.Message}"
                };
            }
        }

        private static string GenerateFenceEventMessage(string deviceName, string stationName, FenceRecord record)
        {
            var eventDescription = record.EventType.GetDescription();
            return $"{stationName} - {deviceName} 發生 {eventDescription}";
        }
    }
}