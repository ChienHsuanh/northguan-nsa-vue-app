using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class CrowdOverviewService : ICrowdOverviewService
    {
        private readonly ApplicationDbContext _context;

        public CrowdOverviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CrowdCapacityHistoryResponse> GetRecentCapacityHistoryAsync(int stationId, int timeRange, List<int> availableStationIds)
        {
            try
            {
                // 驗證 timeRange 範圍 (60秒到30天)
                if (timeRange < 60 || timeRange > 2592000)
                {
                    return new CrowdCapacityHistoryResponse
                    {
                        Success = false,
                        Message = "時間範圍必須在60秒到2592000秒(30天)之間"
                    };
                }
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.CrowdRecords
                            join device in _context.CrowdDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            orderby record.Time descending
                            select new { record, device, station };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(r => new CrowdHistoryData
                {
                    Time = r.record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)r.record.Time).ToUnixTimeMilliseconds(),
                    PeopleCount = r.record.Count,
                    Area = r.device.Area,
                    Density = r.device.Area > 0 ? Math.Round((double)r.record.Count / r.device.Area, 4) : 0,
                    StationName = r.station.Name,
                    DeviceName = r.device.Name
                }).ToList();

                return new CrowdCapacityHistoryResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new CrowdCapacityHistoryResponse
                {
                    Success = false,
                    Message = $"Error retrieving crowd capacity history: {ex.Message}"
                };
            }
        }

        public async Task<CrowdCapacityRateResponse> GetRecentCapacityRateAsync(int stationId, int timeRange, int limit, List<int> availableStationIds)
        {
            try
            {
                // 驗證 timeRange 範圍 (60秒到30天)
                if (timeRange < 60 || timeRange > 2592000)
                {
                    return new CrowdCapacityRateResponse
                    {
                        Success = false,
                        Message = "時間範圍必須在60秒到2592000秒(30天)之間"
                    };
                }
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.CrowdRecords
                            join device in _context.CrowdDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            group new { record, device, station } by new { device.Id, DeviceName = device.Name, StationName = station.Name, device.Area } into g
                            select new
                            {
                                DeviceName = g.Key.DeviceName,
                                StationName = g.Key.StationName,
                                Area = g.Key.Area,
                                Records = g.ToList()
                            };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(g => new CrowdRateData
                {
                    DeviceId = g.Records.First().device.Id,
                    StationId = g.Records.First().station.Id,
                    StationName = g.StationName,
                    DeviceName = g.DeviceName,
                    AverageDensity = g.Area > 0 ? Math.Round(g.Records.Average(r => (double)r.record.Count / g.Area), 4) : 0,
                    Rate = g.Area > 0 ? Math.Round(g.Records.Average(r => (double)r.record.Count / g.Area) * 100, 2) : 0,
                    Status = g.Records.First().device.Status ?? "online",
                    AveragePeopleCount = (int)Math.Round(g.Records.Average(r => r.record.Count)),
                    TotalRecords = g.Records.Count,
                    LatestTime = g.Records.Any() ? g.Records.Max(r => r.record.Time).ToString("yyyy-MM-dd HH:mm:ss") : ""
                })
                .OrderByDescending(x => x.AverageDensity)
                .Take(limit)
                .ToList();

                return new CrowdCapacityRateResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new CrowdCapacityRateResponse
                {
                    Success = false,
                    Message = $"Error retrieving crowd capacity rates: {ex.Message}"
                };
            }
        }
    }
}