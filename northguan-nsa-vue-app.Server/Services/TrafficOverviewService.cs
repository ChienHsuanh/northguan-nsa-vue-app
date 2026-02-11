using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class TrafficOverviewService : ITrafficOverviewService
    {
        private readonly ApplicationDbContext _context;

        public TrafficOverviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrafficRoadConditionHistoryResponse> GetRecentRoadConditionHistoryAsync(int stationId, int timeRange, List<int> availableStationIds)
        {
            try
            {
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.TrafficRecords
                            join device in _context.TrafficDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            orderby record.Time descending
                            select new { record, device, station };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(r => new TrafficHistoryData
                {
                    Time = r.record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)r.record.Time).ToUnixTimeMilliseconds(),
                    VehicleCount = r.record.VehicleCount,
                    AverageSpeed = r.record.AverageSpeed,
                    SpeedStatus = r.record.AverageSpeed > r.device.SpeedLimit ? "超速" : "正常",
                    StationName = r.station.Name,
                    DeviceName = r.device.Name
                }).ToList();

                return new TrafficRoadConditionHistoryResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new TrafficRoadConditionHistoryResponse
                {
                    Success = false,
                    Message = $"Error retrieving traffic road condition history: {ex.Message}"
                };
            }
        }

        public async Task<TrafficRoadConditionResponse> GetRecentRoadConditionAsync(int stationId, int timeRange, int limit, List<int> availableStationIds)
        {
            try
            {
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.TrafficRecords
                            join device in _context.TrafficDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            group new { record, device, station } by new { device.Id, DeviceName = device.Name, StationName = station.Name, device.SpeedLimit } into g
                            select new
                            {
                                DeviceName = g.Key.DeviceName,
                                StationName = g.Key.StationName,
                                SpeedLimit = g.Key.SpeedLimit,
                                Records = g.ToList()
                            };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(g => new TrafficConditionData
                {
                    DeviceId = g.Records.First().device.Id,
                    StationId = g.Records.First().station.Id,
                    StationName = g.StationName,
                    DeviceName = g.DeviceName,
                    AverageSpeed = Math.Round(g.Records.Average(r => r.record.AverageSpeed), 2),
                    Rate = (double)Math.Round(g.Records.Average(r => r.record.AverageSpeed), 2),
                    Status = g.Records.First().device.Status ?? "online",
                    AverageVehicleCount = (int)Math.Round(g.Records.Average(r => r.record.VehicleCount)),
                    SpeedLimit = g.SpeedLimit,
                    SpeedStatus = g.Records.Average(r => r.record.AverageSpeed) > g.SpeedLimit ? "超速" : "正常",
                    TotalRecords = g.Records.Count,
                    LatestTime = g.Records.Any() ? g.Records.Max(r => r.record.Time).ToString("yyyy-MM-dd HH:mm:ss") : ""
                })
                .OrderByDescending(x => x.AverageSpeed)
                .Take(limit)
                .ToList();

                return new TrafficRoadConditionResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new TrafficRoadConditionResponse
                {
                    Success = false,
                    Message = $"Error retrieving traffic road conditions: {ex.Message}"
                };
            }
        }
    }
}