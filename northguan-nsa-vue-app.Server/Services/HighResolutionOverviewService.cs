using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class HighResolutionOverviewService : IHighResolutionOverviewService
    {
        private readonly ApplicationDbContext _context;

        public HighResolutionOverviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HighResolutionOverviewResponse> GetOverviewInfoAsync(int stationId, List<int> availableStationIds)
        {
            try
            {
                var query = from device in _context.HighResolutionDevices
                           join station in _context.Stations on device.StationId equals station.Id
                           where availableStationIds.Contains(station.Id)
                                 && (stationId == 0 || station.Id == stationId)
                                 && device.DeletedAt == null
                           select new { device, station };

                var results = await query.AsNoTracking().ToListAsync();

                var data = results.Select(r => new HighResolutionData
                {
                    StationName = r.station.Name,
                    DeviceName = r.device.Name,
                    VideoUrl = r.device.VideoUrl ?? "",
                    Status = r.device.Status ?? "未知",
                    LatestOnlineTime = r.device.LatestOnlineTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    Lat = r.device.Lat,
                    Lng = r.device.Lng
                }).ToList();

                return new HighResolutionOverviewResponse
                {
                    Data = data,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new HighResolutionOverviewResponse
                {
                    Success = false,
                    Message = $"Error retrieving high resolution overview: {ex.Message}"
                };
            }
        }
    }
}