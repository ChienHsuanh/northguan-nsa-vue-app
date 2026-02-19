using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;

namespace northguan_nsa_vue_app.Server.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly ApplicationDbContext _context;

        public DeviceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeviceListResponse>> GetDevicesAsync(string type, string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            switch (type.ToLower())
            {
                case "crowd":
                    return await GetCrowdDevicesAsync(keyword, page, size, availableStationIds);
                case "parking":
                    return await GetParkingDevicesAsync(keyword, page, size, availableStationIds);
                case "traffic":
                    return await GetTrafficDevicesAsync(keyword, page, size, availableStationIds);
                case "fence":
                    return await GetFenceDevicesAsync(keyword, page, size, availableStationIds);
                case "highresolution":
                    return await GetHighResolutionDevicesAsync(keyword, page, size, availableStationIds);
                case "water":
                    return await GetWaterDevicesAsync(keyword, page, size, availableStationIds);
                default:
                    var devices = new List<DeviceListResponse>();
                    devices.AddRange(await GetCrowdDevicesAsync(keyword, page, size, availableStationIds));
                    devices.AddRange(await GetParkingDevicesAsync(keyword, page, size, availableStationIds));
                    devices.AddRange(await GetTrafficDevicesAsync(keyword, page, size, availableStationIds));
                    devices.AddRange(await GetFenceDevicesAsync(keyword, page, size, availableStationIds));
                    devices.AddRange(await GetHighResolutionDevicesAsync(keyword, page, size, availableStationIds));
                    devices.AddRange(await GetWaterDevicesAsync(keyword, page, size, availableStationIds));
                    return devices;
            }
        }

        private async Task<List<DeviceListResponse>> GetCrowdDevicesAsync(string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            var query = _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.Name.Contains(keyword));

            if (availableStationIds != null)
                query = query.Where(d => availableStationIds.Contains(d.StationId));

            var orderedQuery = query.OrderBy(d => d.StationId);

            if (page.HasValue && size.HasValue)
            {
                orderedQuery = (IOrderedQueryable<CrowdDevice>)orderedQuery
                    .Skip((page.Value - 1) * size.Value)
                    .Take(size.Value);
            }

            return await orderedQuery
                .Select(d => new DeviceListResponse
                {
                    Type = "crowd",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Status = d.Status,
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Area = d.Area,
                    VideoUrl = d.VideoUrl,
                    ApiUrl = d.ApiUrl,
                    CameraConfig = d.CameraConfig
                })
                .ToListAsync();
        }

        private async Task<List<DeviceListResponse>> GetParkingDevicesAsync(string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            var query = _context.ParkingDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.Name.Contains(keyword));

            if (availableStationIds != null)
                query = query.Where(d => availableStationIds.Contains(d.StationId));

            var orderedQuery = query.OrderBy(d => d.StationId);

            if (page.HasValue && size.HasValue)
            {
                orderedQuery = (IOrderedQueryable<ParkingDevice>)orderedQuery
                    .Skip((page.Value - 1) * size.Value)
                    .Take(size.Value);
            }

            return await orderedQuery
                .Select(d => new DeviceListResponse
                {
                    Type = "parking",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Status = d.Status,
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ApiUrl = d.ApiUrl,
                    NumberOfParking = d.NumberOfParking,
                    CameraConfig = d.CameraConfig
                })
                .ToListAsync();
        }

        private async Task<List<DeviceListResponse>> GetTrafficDevicesAsync(string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            var query = _context.TrafficDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.Name.Contains(keyword));

            if (availableStationIds != null)
                query = query.Where(d => availableStationIds.Contains(d.StationId));

            var orderedQuery = query.OrderBy(d => d.StationId);

            if (page.HasValue && size.HasValue)
            {
                orderedQuery = (IOrderedQueryable<TrafficDevice>)orderedQuery
                    .Skip((page.Value - 1) * size.Value)
                    .Take(size.Value);
            }

            return await orderedQuery
                .Select(d => new DeviceListResponse
                {
                    Type = "traffic",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Status = d.Status,
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    City = d.City,
                    ETagNumber = d.ETagNumber,
                    SpeedLimit = d.SpeedLimit,
                    CameraConfig = d.CameraConfig
                })
                .ToListAsync();
        }

        private async Task<List<DeviceListResponse>> GetFenceDevicesAsync(string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            var query = _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.Name.Contains(keyword));

            if (availableStationIds != null)
                query = query.Where(d => availableStationIds.Contains(d.StationId));

            var orderedQuery = query.OrderBy(d => d.StationId);

            if (page.HasValue && size.HasValue)
            {
                orderedQuery = (IOrderedQueryable<FenceDevice>)orderedQuery
                    .Skip((page.Value - 1) * size.Value)
                    .Take(size.Value);
            }

            return await orderedQuery
                .Select(d => new DeviceListResponse
                {
                    Type = "fence",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Status = d.Status,
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ObservingTimeStart = d.ObservingTimeStart,
                    ObservingTimeEnd = d.ObservingTimeEnd,
                    VideoUrl = d.VideoUrl,
                    Zones = d.Zones,
                    CameraConfig = d.CameraConfig
                })
                .ToListAsync();
        }

        private async Task<List<DeviceListResponse>> GetHighResolutionDevicesAsync(string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            var query = _context.HighResolutionDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.Name.Contains(keyword));

            if (availableStationIds != null)
                query = query.Where(d => availableStationIds.Contains(d.StationId));

            var orderedQuery = query.OrderBy(d => d.StationId);

            if (page.HasValue && size.HasValue)
            {
                orderedQuery = (IOrderedQueryable<HighResolutionDevice>)orderedQuery
                    .Skip((page.Value - 1) * size.Value)
                    .Take(size.Value);
            }

            return await orderedQuery
                .Select(d => new DeviceListResponse
                {
                    Type = "highResolution",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Status = d.Status,
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    VideoUrl = d.VideoUrl,
                    CameraConfig = d.CameraConfig
                })
                .ToListAsync();
        }

        private async Task<List<DeviceListResponse>> GetWaterDevicesAsync(string keyword, int? page, int? size, List<int>? availableStationIds)
        {
            var query = _context.WaterDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.Name.Contains(keyword));

            if (availableStationIds != null)
                query = query.Where(d => availableStationIds.Contains(d.StationId));

            var orderedQuery = query.OrderBy(d => d.StationId);

            if (page.HasValue && size.HasValue)
            {
                orderedQuery = (IOrderedQueryable<WaterDevice>)orderedQuery
                    .Skip((page.Value - 1) * size.Value)
                    .Take(size.Value);
            }

            return await orderedQuery
                .Select(d => new DeviceListResponse
                {
                    Type = "water",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Status = d.Status,
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    VideoUrl = d.VideoUrl,
                    CameraConfig = d.CameraConfig
                })
                .ToListAsync();
        }

        public async Task<int> GetDeviceCountAsync(string type, string keyword, List<int>? availableStationIds)
        {
            switch (type.ToLower())
            {
                case "crowd":
                    var crowdQuery = _context.CrowdDevices.Include(d => d.Station).Where(d => d.Station.DeletedAt == null);
                    if (!string.IsNullOrEmpty(keyword)) crowdQuery = crowdQuery.Where(d => d.Name.Contains(keyword));
                    if (availableStationIds != null) crowdQuery = crowdQuery.Where(d => availableStationIds.Contains(d.StationId));
                    return await crowdQuery.CountAsync();

                case "parking":
                    var parkingQuery = _context.ParkingDevices.Include(d => d.Station).Where(d => d.Station.DeletedAt == null);
                    if (!string.IsNullOrEmpty(keyword)) parkingQuery = parkingQuery.Where(d => d.Name.Contains(keyword));
                    if (availableStationIds != null) parkingQuery = parkingQuery.Where(d => availableStationIds.Contains(d.StationId));
                    return await parkingQuery.CountAsync();

                case "traffic":
                    var trafficQuery = _context.TrafficDevices.Include(d => d.Station).Where(d => d.Station.DeletedAt == null);
                    if (!string.IsNullOrEmpty(keyword)) trafficQuery = trafficQuery.Where(d => d.Name.Contains(keyword));
                    if (availableStationIds != null) trafficQuery = trafficQuery.Where(d => availableStationIds.Contains(d.StationId));
                    return await trafficQuery.CountAsync();

                case "fence":
                    var fenceQuery = _context.FenceDevices.Include(d => d.Station).Where(d => d.Station.DeletedAt == null);
                    if (!string.IsNullOrEmpty(keyword)) fenceQuery = fenceQuery.Where(d => d.Name.Contains(keyword));
                    if (availableStationIds != null) fenceQuery = fenceQuery.Where(d => availableStationIds.Contains(d.StationId));
                    return await fenceQuery.CountAsync();

                case "highresolution":
                    var hrQuery = _context.HighResolutionDevices.Include(d => d.Station).Where(d => d.Station.DeletedAt == null);
                    if (!string.IsNullOrEmpty(keyword)) hrQuery = hrQuery.Where(d => d.Name.Contains(keyword));
                    if (availableStationIds != null) hrQuery = hrQuery.Where(d => availableStationIds.Contains(d.StationId));
                    return await hrQuery.CountAsync();

                case "water":
                    var waterQuery = _context.WaterDevices.Include(d => d.Station).Where(d => d.Station.DeletedAt == null);
                    if (!string.IsNullOrEmpty(keyword)) waterQuery = waterQuery.Where(d => d.Name.Contains(keyword));
                    if (availableStationIds != null) waterQuery = waterQuery.Where(d => availableStationIds.Contains(d.StationId));
                    return await waterQuery.CountAsync();

                default:
                    return 0;
            }
        }

        public async Task<DeviceListResponse?> GetDeviceByIdAsync(int deviceId)
        {
            // Try to find device in each type table
            var crowdDevice = await _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null && d.Id == deviceId)
                .Select(d => new DeviceListResponse
                {
                    Type = "crowd",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    Area = d.Area,
                    VideoUrl = d.VideoUrl,
                    ApiUrl = d.ApiUrl,
                    CameraConfig = d.CameraConfig
                })
                .FirstOrDefaultAsync();

            if (crowdDevice != null) return crowdDevice;

            var parkingDevice = await _context.ParkingDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null && d.Id == deviceId)
                .Select(d => new DeviceListResponse
                {
                    Type = "parking",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    ApiUrl = d.ApiUrl,
                    NumberOfParking = d.NumberOfParking,
                    CameraConfig = d.CameraConfig
                })
                .FirstOrDefaultAsync();

            if (parkingDevice != null) return parkingDevice;

            var trafficDevice = await _context.TrafficDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null && d.Id == deviceId)
                .Select(d => new DeviceListResponse
                {
                    Type = "traffic",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    City = d.City,
                    ETagNumber = d.ETagNumber,
                    SpeedLimit = d.SpeedLimit,
                    CameraConfig = d.CameraConfig
                })
                .FirstOrDefaultAsync();

            if (trafficDevice != null) return trafficDevice;

            var fenceDevice = await _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null && d.Id == deviceId)
                .Select(d => new DeviceListResponse
                {
                    Type = "fence",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    ObservingTimeStart = d.ObservingTimeStart,
                    ObservingTimeEnd = d.ObservingTimeEnd,
                    VideoUrl = d.VideoUrl,
                    Zones = d.Zones,
                    CameraConfig = d.CameraConfig
                })
                .FirstOrDefaultAsync();

            if (fenceDevice != null) return fenceDevice;

            var hrDevice = await _context.HighResolutionDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null && d.Id == deviceId)
                .Select(d => new DeviceListResponse
                {
                    Type = "highresolution",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    VideoUrl = d.VideoUrl,
                    CameraConfig = d.CameraConfig
                })
                .FirstOrDefaultAsync();

            if (hrDevice != null) return hrDevice;

            var waterDevice = await _context.WaterDevices
                .Include(d => d.Station)
                .Where(d => d.Station.DeletedAt == null && d.Id == deviceId)
                .Select(d => new DeviceListResponse
                {
                    Type = "water",
                    Id = d.Id,
                    Name = d.Name,
                    Lat = d.Lat,
                    Lng = d.Lng,
                    StationID = d.StationId,
                    Serial = d.Serial,
                    StationName = d.Station.Name,
                    VideoUrl = d.VideoUrl,
                    CameraConfig = d.CameraConfig
                })
                .FirstOrDefaultAsync();

            return waterDevice;
        }

        public async Task CreateDeviceAsync(CreateDeviceRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    switch (request.Type.ToLower())
                    {
                        case "crowd":
                            var crowdDevice = new CrowdDevice
                            {
                                StationId = request.StationID,
                                Name = request.Name,
                                Lng = request.Lng,
                                Lat = request.Lat,
                                Serial = request.Serial,
                                Area = request.Area ?? 0,
                                ApiUrl = request.ApiUrl,
                                VideoUrl = request.VideoUrl,
                                CameraConfig = request.CameraConfig
                            };
                            _context.CrowdDevices.Add(crowdDevice);
                            break;

                        case "parking":
                            var parkingDevice = new ParkingDevice
                            {
                                StationId = request.StationID,
                                Name = request.Name,
                                Lng = request.Lng,
                                Lat = request.Lat,
                                Serial = request.Serial,
                                ApiUrl = request.ApiUrl,
                                NumberOfParking = request.NumberOfParking ?? 0,
                                CameraConfig = request.CameraConfig
                            };
                            _context.ParkingDevices.Add(parkingDevice);
                            break;

                        case "traffic":
                            var trafficDevice = new TrafficDevice
                            {
                                StationId = request.StationID,
                                Name = request.Name,
                                Lng = request.Lng,
                                Lat = request.Lat,
                                Serial = request.Serial,
                                SpeedLimit = request.SpeedLimit ?? 0,
                                City = request.City,
                                ETagNumber = request.ETagNumber,
                                CameraConfig = request.CameraConfig
                            };
                            _context.TrafficDevices.Add(trafficDevice);
                            break;

                        case "fence":
                            var fenceDevice = new FenceDevice
                            {
                                StationId = request.StationID,
                                Name = request.Name,
                                Lng = request.Lng,
                                Lat = request.Lat,
                                Serial = request.Serial,
                                ObservingTimeStart = request.ObservingTimeStart,
                                ObservingTimeEnd = request.ObservingTimeEnd,
                                VideoUrl = request.VideoUrl,
                                CameraConfig = request.CameraConfig
                            };
                            _context.FenceDevices.Add(fenceDevice);
                            break;

                        case "highresolution":
                            var hrDevice = new HighResolutionDevice
                            {
                                StationId = request.StationID,
                                Name = request.Name,
                                Lng = request.Lng,
                                Lat = request.Lat,
                                Serial = request.Serial,
                                VideoUrl = request.VideoUrl,
                                CameraConfig = request.CameraConfig
                            };
                            _context.HighResolutionDevices.Add(hrDevice);
                            break;

                        case "water":
                            var waterDevice = new WaterDevice
                            {
                                StationId = request.StationID,
                                Name = request.Name,
                                Lng = request.Lng,
                                Lat = request.Lat,
                                Serial = request.Serial,
                                VideoUrl = request.VideoUrl,
                                CameraConfig = request.CameraConfig
                            };
                            _context.WaterDevices.Add(waterDevice);
                            break;

                        default:
                            throw new ArgumentException($"不支援的裝置類型: {request.Type}");
                    }

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

        public async Task UpdateDeviceAsync(int id, UpdateDeviceRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    switch (request.Type.ToLower())
                    {
                        case "crowd":
                            var crowdDevice = await _context.CrowdDevices.FindAsync(id);
                            if (crowdDevice == null) throw new KeyNotFoundException("找不到指定的人流裝置");

                            crowdDevice.Name = request.Name;
                            crowdDevice.Lng = request.Lng;
                            crowdDevice.Lat = request.Lat;
                            if (crowdDevice.Serial != request.Serial)
                            {
                                // Serial 是 principal key，需要先更新關聯紀錄再改 Serial
                                var oldSerial = crowdDevice.Serial;
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE CrowdRecords SET DeviceSerial = {0} WHERE DeviceSerial = {1}",
                                    request.Serial, oldSerial);
                                // 先 SaveChanges 清除 EF 追蹤狀態
                                await _context.SaveChangesAsync();
                                // 用原生 SQL 更新 Serial（繞過 EF alternate key 限制）
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE CrowdDevices SET Serial = {0} WHERE Id = {1}",
                                    request.Serial, id);
                                // 重新載入 entity
                                await _context.Entry(crowdDevice).ReloadAsync();
                            }
                            crowdDevice.Area = request.Area ?? 0;
                            crowdDevice.ApiUrl = request.ApiUrl;
                            crowdDevice.VideoUrl = request.VideoUrl;
                            crowdDevice.CameraConfig = request.CameraConfig;
                            break;

                        case "parking":
                            var parkingDevice = await _context.ParkingDevices.FindAsync(id);
                            if (parkingDevice == null) throw new KeyNotFoundException("找不到指定的停車裝置");

                            parkingDevice.Name = request.Name;
                            parkingDevice.Lng = request.Lng;
                            parkingDevice.Lat = request.Lat;
                            if (parkingDevice.Serial != request.Serial)
                            {
                                var oldSerial = parkingDevice.Serial;
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE ParkingRecords SET DeviceSerial = {0} WHERE DeviceSerial = {1}",
                                    request.Serial, oldSerial);
                                await _context.SaveChangesAsync();
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE ParkingDevices SET Serial = {0} WHERE Id = {1}",
                                    request.Serial, id);
                                await _context.Entry(parkingDevice).ReloadAsync();
                            }
                            parkingDevice.ApiUrl = request.ApiUrl;
                            parkingDevice.NumberOfParking = request.NumberOfParking ?? 0;
                            parkingDevice.CameraConfig = request.CameraConfig;
                            break;

                        case "traffic":
                            var trafficDevice = await _context.TrafficDevices.FindAsync(id);
                            if (trafficDevice == null) throw new KeyNotFoundException("找不到指定的車流裝置");

                            trafficDevice.Name = request.Name;
                            trafficDevice.Lng = request.Lng;
                            trafficDevice.Lat = request.Lat;
                            if (trafficDevice.Serial != request.Serial)
                            {
                                var oldSerial = trafficDevice.Serial;
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE TrafficRecords SET DeviceSerial = {0} WHERE DeviceSerial = {1}",
                                    request.Serial, oldSerial);
                                await _context.SaveChangesAsync();
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE TrafficDevices SET Serial = {0} WHERE Id = {1}",
                                    request.Serial, id);
                                await _context.Entry(trafficDevice).ReloadAsync();
                            }
                            trafficDevice.SpeedLimit = request.SpeedLimit ?? 0;
                            trafficDevice.City = request.City;
                            trafficDevice.ETagNumber = request.ETagNumber;
                            trafficDevice.CameraConfig = request.CameraConfig;
                            break;

                        case "fence":
                            var fenceDevice = await _context.FenceDevices.FindAsync(id);
                            if (fenceDevice == null) throw new KeyNotFoundException("找不到指定的電子圍籬裝置");

                            fenceDevice.Name = request.Name;
                            fenceDevice.Lng = request.Lng;
                            fenceDevice.Lat = request.Lat;
                            if (fenceDevice.Serial != request.Serial)
                            {
                                var oldSerial = fenceDevice.Serial;
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE FenceRecords SET DeviceSerial = {0} WHERE DeviceSerial = {1}",
                                    request.Serial, oldSerial);
                                await _context.SaveChangesAsync();
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE FenceDevices SET Serial = {0} WHERE Id = {1}",
                                    request.Serial, id);
                                await _context.Entry(fenceDevice).ReloadAsync();
                            }
                            fenceDevice.ObservingTimeStart = request.ObservingTimeStart;
                            fenceDevice.ObservingTimeEnd = request.ObservingTimeEnd;
                            fenceDevice.VideoUrl = request.VideoUrl;
                            fenceDevice.CameraConfig = request.CameraConfig;
                            break;

                        case "highresolution":
                            var hrDevice = await _context.HighResolutionDevices.FindAsync(id);
                            if (hrDevice == null) throw new KeyNotFoundException("找不到指定的高畫質裝置");

                            hrDevice.Name = request.Name;
                            hrDevice.Lng = request.Lng;
                            hrDevice.Lat = request.Lat;
                            if (hrDevice.Serial != request.Serial)
                            {
                                var oldSerial = hrDevice.Serial;
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE HighResolutionRecords SET DeviceSerial = {0} WHERE DeviceSerial = {1}",
                                    request.Serial, oldSerial);
                                await _context.SaveChangesAsync();
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE HighResolutionDevices SET Serial = {0} WHERE Id = {1}",
                                    request.Serial, id);
                                await _context.Entry(hrDevice).ReloadAsync();
                            }
                            hrDevice.VideoUrl = request.VideoUrl;
                            hrDevice.CameraConfig = request.CameraConfig;
                            break;

                        case "water":
                            var waterDevice = await _context.WaterDevices.FindAsync(id);
                            if (waterDevice == null) throw new KeyNotFoundException("找不到指定的水域監控裝置");

                            waterDevice.Name = request.Name;
                            waterDevice.Lng = request.Lng;
                            waterDevice.Lat = request.Lat;
                            if (waterDevice.Serial != request.Serial)
                            {
                                var oldSerial = waterDevice.Serial;
                                await _context.SaveChangesAsync();
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE WaterDevices SET Serial = {0} WHERE Id = {1}",
                                    request.Serial, id);
                                await _context.Entry(waterDevice).ReloadAsync();
                            }
                            waterDevice.VideoUrl = request.VideoUrl;
                            waterDevice.CameraConfig = request.CameraConfig;
                            break;

                        default:
                            throw new ArgumentException($"不支援的裝置類型: {request.Type}");
                    }

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

        public async Task DeleteDeviceAsync(int id, string type)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    switch (type.ToLower())
                    {
                        case "crowd":
                            var crowdDevice = await _context.CrowdDevices.FindAsync(id);
                            if (crowdDevice != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "DELETE FROM CrowdRecords WHERE DeviceSerial = {0}", crowdDevice.Serial);
                                _context.CrowdDevices.Remove(crowdDevice);
                            }
                            break;

                        case "parking":
                            var parkingDevice = await _context.ParkingDevices.FindAsync(id);
                            if (parkingDevice != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "DELETE FROM ParkingRecords WHERE DeviceSerial = {0}", parkingDevice.Serial);
                                _context.ParkingDevices.Remove(parkingDevice);
                            }
                            break;

                        case "traffic":
                            var trafficDevice = await _context.TrafficDevices.FindAsync(id);
                            if (trafficDevice != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "DELETE FROM TrafficRecords WHERE DeviceSerial = {0}", trafficDevice.Serial);
                                _context.TrafficDevices.Remove(trafficDevice);
                            }
                            break;

                        case "fence":
                            var fenceDevice = await _context.FenceDevices.FindAsync(id);
                            if (fenceDevice != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "DELETE FROM FenceRecords WHERE DeviceSerial = {0}", fenceDevice.Serial);
                                _context.FenceDevices.Remove(fenceDevice);
                            }
                            break;

                        case "highresolution":
                            var hrDevice = await _context.HighResolutionDevices.FindAsync(id);
                            if (hrDevice != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "DELETE FROM HighResolutionRecords WHERE DeviceSerial = {0}", hrDevice.Serial);
                                _context.HighResolutionDevices.Remove(hrDevice);
                            }
                            break;

                        case "water":
                            var waterDevice = await _context.WaterDevices.FindAsync(id);
                            if (waterDevice != null)
                            {
                                _context.WaterDevices.Remove(waterDevice);
                            }
                            break;

                        default:
                            throw new ArgumentException($"不支援的裝置類型: {type}");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // 資料已被其他請求刪除，視為成功
                    await transaction.RollbackAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // Device-specific methods
        public async Task<List<CrowdDevice>> GetCrowdDevicesByStationIdsAsync(List<int> stationIds)
        {
            return await _context.CrowdDevices
                .Where(d => stationIds.Contains(d.StationId))
                .ToListAsync();
        }

        public async Task<List<ParkingDevice>> GetParkingDevicesByStationIdsAsync(List<int> stationIds)
        {
            return await _context.ParkingDevices
                .Where(d => stationIds.Contains(d.StationId))
                .ToListAsync();
        }

        public async Task<List<TrafficDevice>> GetTrafficDevicesByStationIdsAsync(List<int> stationIds)
        {
            return await _context.TrafficDevices
                .Where(d => stationIds.Contains(d.StationId))
                .ToListAsync();
        }

        public async Task<List<FenceDevice>> GetFenceDevicesByStationIdsAsync(List<int> stationIds)
        {
            return await _context.FenceDevices
                .Where(d => stationIds.Contains(d.StationId))
                .ToListAsync();
        }

        public async Task<List<HighResolutionDevice>> GetHighResolutionDevicesByStationIdsAsync(List<int> stationIds)
        {
            return await _context.HighResolutionDevices
                .Where(d => stationIds.Contains(d.StationId))
                .ToListAsync();
        }

        public async Task<List<WaterDevice>> GetWaterDevicesByStationIdsAsync(List<int> stationIds)
        {
            return await _context.WaterDevices
                .Where(d => stationIds.Contains(d.StationId))
                .ToListAsync();
        }

        public async Task<List<DeviceStatusResponse>> GetDevicesStatusAsync(int? page, int? size, string keyword, List<int> availableStationIds)
        {
            var devices = new List<DeviceStatusResponse>();

            // Add crowd devices
            var crowdDevices = await _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) &&
                           (string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword)))
                .Select(d => new DeviceStatusResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Serial = d.Serial,
                    Status = d.Status ?? "unknown",
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ?
                        d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    StationName = d.Station.Name,
                    Type = "crowd"
                })
                .ToListAsync();

            devices.AddRange(crowdDevices);

            // Add parking devices
            var parkingDevices = await _context.ParkingDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) &&
                           (string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword)))
                .Select(d => new DeviceStatusResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Serial = d.Serial,
                    Status = d.Status ?? "unknown",
                    LatestOnlineTime = d.LatestOnlineTime.HasValue
                        ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    StationName = d.Station.Name,
                    Type = "parking"
                })
                .ToListAsync();

            devices.AddRange(parkingDevices);

            // Add traffic devices
            var trafficDevices = await _context.TrafficDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) &&
                           (string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword)))
                .Select(d => new DeviceStatusResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Serial = d.Serial,
                    Status = d.Status ?? "unknown",
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    StationName = d.Station.Name,
                    Type = "traffic"
                })
                .ToListAsync();

            devices.AddRange(trafficDevices);

            // Add fence devices
            var fenceDevices = await _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) &&
                           (string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword)))
                .Select(d => new DeviceStatusResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Serial = d.Serial,
                    Status = d.Status ?? "unknown",
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    StationName = d.Station.Name,
                    Type = "fence"
                })
                .ToListAsync();

            devices.AddRange(fenceDevices);

            // Add high resolution devices
            var hrDevices = await _context.HighResolutionDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) &&
                           (string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword)))
                .Select(d => new DeviceStatusResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Serial = d.Serial,
                    Status = d.Status ?? "unknown",
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    StationName = d.Station.Name,
                    Type = "highresolution"
                })
                .ToListAsync();

            devices.AddRange(hrDevices);

            // Add water devices
            var waterDevices = await _context.WaterDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) &&
                           (string.IsNullOrEmpty(keyword) || d.Name.Contains(keyword)))
                .Select(d => new DeviceStatusResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Serial = d.Serial,
                    Status = d.Status ?? "unknown",
                    LatestOnlineTime = d.LatestOnlineTime.HasValue ? d.LatestOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    StationName = d.Station.Name,
                    Type = "water"
                })
                .ToListAsync();

            devices.AddRange(waterDevices);

            if (page.HasValue && size.HasValue)
            {
                return devices
                    .Skip((page.Value - 1) * size.Value)
        .Take(size.Value)
                    .ToList();
            }

            return devices;
        }

        public async Task<int> GetDevicesStatusCountAsync(List<int> availableStationIds)
        {
            var crowdCount = await _context.CrowdDevices.Where(d => availableStationIds.Contains(d.StationId)).CountAsync();
            var parkingCount = await _context.ParkingDevices.Where(d => availableStationIds.Contains(d.StationId)).CountAsync();
            var trafficCount = await _context.TrafficDevices.Where(d => availableStationIds.Contains(d.StationId)).CountAsync();
            var fenceCount = await _context.FenceDevices.Where(d => availableStationIds.Contains(d.StationId)).CountAsync();
            var hrCount = await _context.HighResolutionDevices.Where(d => availableStationIds.Contains(d.StationId)).CountAsync();
            var waterCount = await _context.WaterDevices.Where(d => availableStationIds.Contains(d.StationId)).CountAsync();

            return crowdCount + parkingCount + trafficCount + fenceCount + hrCount + waterCount;
        }

        public async Task<List<DeviceStatusLogResponse>> GetDeviceStatusLogsAsync(int page, int size, string keyword, List<int> availableStationIds)
        {
            // This would need to be implemented based on your device status log requirements
            return await _context.DeviceStatusLogs
                .Where(l => string.IsNullOrEmpty(keyword) || l.DeviceSerial.Contains(keyword))
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(l => new DeviceStatusLogResponse
                {
                    Id = l.Id,
                    DeviceType = l.DeviceType,
                    DeviceSerial = l.DeviceSerial,
                    Status = l.Status,
                    Time = l.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    DeviceName = "", // You'd need to join with device tables to get this
                    StationName = "" // You'd need to join with station table to get this
                })
                .ToListAsync();
        }

        public async Task<int> GetDeviceStatusLogCountAsync(string keyword, List<int> availableStationIds)
        {
            return await _context.DeviceStatusLogs
                .Where(l => string.IsNullOrEmpty(keyword) || l.DeviceSerial.Contains(keyword))
                .CountAsync();
        }
    }
}