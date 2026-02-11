using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class ParkingOverviewService : IParkingOverviewService
    {
        private readonly ApplicationDbContext _context;

        public ParkingOverviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ParkingConversionHistoryResponse> GetRecentConversionHistoryAsync(int stationId, int timeRange, List<int> availableStationIds)
        {
            try
            {
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.ParkingRecords
                            join device in _context.ParkingDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            orderby record.Time descending
                            select new { record, device, station };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(r => new ParkingHistoryData
                {
                    Time = r.record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)r.record.Time).ToUnixTimeMilliseconds(),
                    TotalSpaces = r.device.NumberOfParking,
                    ParkedNum = r.record.OccupiedSpaces,
                    AvailableSpaces = Math.Max(0, r.device.NumberOfParking - r.record.OccupiedSpaces),
                    OccupancyRate = r.device.NumberOfParking > 0 ? Math.Round((double)r.record.OccupiedSpaces / r.device.NumberOfParking * 100, 2) : 0,
                    StationName = r.station.Name,
                    DeviceName = r.device.Name
                }).ToList();

                return new ParkingConversionHistoryResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ParkingConversionHistoryResponse
                {
                    Success = false,
                    Message = $"Error retrieving parking conversion history: {ex.Message}"
                };
            }
        }

        public async Task<ParkingRateResponse> GetRecentParkingRateAsync(int stationId, int timeRange, int limit, List<int> availableStationIds)
        {
            try
            {
                var cutoffTime = DateTimeOffset.Now.AddSeconds(-timeRange).ToUnixTimeSeconds();

                var query = from record in _context.ParkingRecords
                            join device in _context.ParkingDevices on record.DeviceSerial equals device.Serial
                            join station in _context.Stations on device.StationId equals station.Id
                            where ((DateTimeOffset)record.Time).ToUnixTimeSeconds() >= cutoffTime
                                  && availableStationIds.Contains(station.Id)
                                  && (stationId == 0 || station.Id == stationId)
                                  && device.DeletedAt == null
                            group new { record, device, station } by new { device.Id, DeviceName = device.Name, StationName = station.Name, device.NumberOfParking } into g
                            select new
                            {
                                DeviceName = g.Key.DeviceName,
                                StationName = g.Key.StationName,
                                TotalSpaces = g.Key.NumberOfParking,
                                Records = g.ToList()
                            };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(g => new ParkingRateData
                {
                    DeviceId = g.Records.First().device.Id,
                    StationId = g.Records.First().station.Id,
                    StationName = g.StationName,
                    DeviceName = g.DeviceName,
                    AverageOccupancyRate = g.TotalSpaces > 0 ? Math.Round(g.Records.Average(r => (double)r.record.OccupiedSpaces / g.TotalSpaces * 100), 2) : 0,
                    Rate = g.TotalSpaces > 0 ? Math.Round(g.Records.Average(r => (double)r.record.OccupiedSpaces / g.TotalSpaces * 100), 2) : 0,
                    Status = g.Records.First().device.Status ?? "online",
                    TotalRecords = g.Records.Count,
                    LatestTime = g.Records.Any() ? g.Records.Max(r => r.record.Time).ToString("yyyy-MM-dd HH:mm:ss") : ""
                })
                .OrderByDescending(x => x.AverageOccupancyRate)
                .Take(limit)
                .ToList();

                return new ParkingRateResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ParkingRateResponse
                {
                    Success = false,
                    Message = $"Error retrieving parking rates: {ex.Message}"
                };
            }
        }
    }
}