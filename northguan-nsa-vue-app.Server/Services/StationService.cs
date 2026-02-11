using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class StationService : IStationService
    {
        private readonly ApplicationDbContext _context;

        public StationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Station>> GetStationsAsync(int page, int size, string keyword, List<int>? availableStationIds)
        {
            var query = _context.Stations.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.Name.Contains(keyword));
            }

            if (availableStationIds != null)
            {
                query = query.Where(s => availableStationIds.Contains(s.Id));
            }

            return await query
                .OrderBy(s => s.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> GetStationCountAsync(string keyword, List<int>? availableStationIds)
        {
            var query = _context.Stations.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.Name.Contains(keyword));
            }

            if (availableStationIds != null)
            {
                query = query.Where(s => availableStationIds.Contains(s.Id));
            }

            return await query.CountAsync();
        }

        public async Task<Station> GetStationByIdAsync(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null)
            {
                throw new KeyNotFoundException($"找不到ID為 {id} 的分站");
            }
            return station;
        }

        public async Task CreateStationAsync(Station station)
        {
            _context.Stations.Add(station);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStationAsync(Station station)
        {
            _context.Stations.Update(station);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStationAsync(int id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Delete related devices first
                    var crowdDevices = await _context.CrowdDevices.Where(d => d.StationId == id).ToListAsync();
                    var parkingDevices = await _context.ParkingDevices.Where(d => d.StationId == id).ToListAsync();
                    var trafficDevices = await _context.TrafficDevices.Where(d => d.StationId == id).ToListAsync();
                    var fenceDevices = await _context.FenceDevices.Where(d => d.StationId == id).ToListAsync();
                    var highResDevices = await _context.HighResolutionDevices.Where(d => d.StationId == id).ToListAsync();

                    _context.CrowdDevices.RemoveRange(crowdDevices);
                    _context.ParkingDevices.RemoveRange(parkingDevices);
                    _context.TrafficDevices.RemoveRange(trafficDevices);
                    _context.FenceDevices.RemoveRange(fenceDevices);
                    _context.HighResolutionDevices.RemoveRange(highResDevices);

                    // Delete station
                    var station = await GetStationByIdAsync(id);
                    _context.Stations.Remove(station);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}