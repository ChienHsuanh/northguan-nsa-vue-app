using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;
using NPOI.SS.Formula.Functions;

namespace northguan_nsa_vue_app.Server.Services
{
    public class MapService : IMapService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IMemoryCache _cache;
        private readonly IFileService _fileService;
        private const int CACHE_TIMEOUT_SECONDS = 30;

        public MapService(ApplicationDbContext context, IAuthService authService, IMemoryCache cache, IFileService fileService)
        {
            _context = context;
            _authService = authService;
            _cache = cache;
            _fileService = fileService;
        }

        public async Task<LandmarksResponse> GetLandmarksAsync()
        {
            var availableStationIds = _authService.GetAvailableStationIds();
            var cacheKey = $"landmarks_{string.Join(",", availableStationIds.OrderBy(x => x))}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_TIMEOUT_SECONDS);
                entry.Size = 1; // 設置快取項目大小

                var landmarks = new List<LandmarkItem>();

                // 人流設備
                var crowdDevices = await _context.CrowdDevices
                    .AsNoTracking()
                    .Include(d => d.Station)
                    .Where(d => availableStationIds.Contains(d.StationId))
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Lat,
                        d.Lng,
                        d.Serial,
                        d.StationId,
                        StationName = d.Station.Name,
                        d.Area,
                        d.VideoUrl,
                        d.Status,
                        d.LatestOnlineTime,
                        d.UpdatedAt
                    })
                    .ToListAsync();

                foreach (var device in crowdDevices)
                {
                    landmarks.Add(new LandmarkItem
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Lat = device.Lat,
                        Lng = device.Lng,
                        Type = "crowd",
                        Status = device.Status ?? "offline",
                        Serial = device.Serial,
                        StationID = device.StationId,
                        StationName = device.StationName,
                        LatestOnlineTime = device.LatestOnlineTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = device.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        Area = device.Area,
                        VideoUrl = device.VideoUrl
                    });
                }

                // 圍籬設備
                var fenceDevices = await _context.FenceDevices
                    .AsNoTracking()
                    .Include(d => d.Station)
                    .Where(d => availableStationIds.Contains(d.StationId))
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Lat,
                        d.Lng,
                        d.Serial,
                        d.StationId,
                        StationName = d.Station.Name,
                        d.ObservingTimeStart,
                        d.ObservingTimeEnd,
                        d.Status
                    })
                    .ToListAsync();

                foreach (var device in fenceDevices)
                {
                    landmarks.Add(new LandmarkItem
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Lat = device.Lat,
                        Lng = device.Lng,
                        Type = "fence",
                        Status = device.Status ?? "offline",
                        Serial = device.Serial,
                        StationID = device.StationId,
                        StationName = device.StationName,
                        ObservingTime = $"{device.ObservingTimeStart} - {device.ObservingTimeEnd}"
                    });
                }

                // 停車場設備
                var parkingDevices = await _context.ParkingDevices
                    .AsNoTracking()
                    .Include(d => d.Station)
                    .Where(d => availableStationIds.Contains(d.StationId))
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Lat,
                        d.Lng,
                        d.Serial,
                        d.StationId,
                        StationName = d.Station.Name,
                        d.NumberOfParking,
                        d.Status
                    })
                    .ToListAsync();

                foreach (var device in parkingDevices)
                {
                    landmarks.Add(new LandmarkItem
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Lat = device.Lat,
                        Lng = device.Lng,
                        Type = "parking",
                        Status = device.Status ?? "offline",
                        Serial = device.Serial,
                        StationID = device.StationId,
                        StationName = device.StationName,
                        NumberOfParking = device.NumberOfParking
                    });
                }

                // 交通設備
                var trafficDevices = await _context.TrafficDevices
                    .AsNoTracking()
                    .Include(d => d.Station)
                    .Where(d => availableStationIds.Contains(d.StationId))
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Lat,
                        d.Lng,
                        d.Serial,
                        d.StationId,
                        StationName = d.Station.Name,
                        d.SpeedLimit,
                        d.Status
                    })
                    .ToListAsync();

                foreach (var device in trafficDevices)
                {
                    landmarks.Add(new LandmarkItem
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Lat = device.Lat,
                        Lng = device.Lng,
                        Type = "traffic",
                        Status = device.Status ?? "offline",
                        Serial = device.Serial,
                        StationID = device.StationId,
                        StationName = device.StationName,
                        SpeedLimit = device.SpeedLimit
                    });
                }

                // 4K影像設備
                var highResDevices = await _context.HighResolutionDevices
                    .AsNoTracking()
                    .Include(d => d.Station)
                    .Where(d => availableStationIds.Contains(d.StationId))
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Lat,
                        d.Lng,
                        d.Serial,
                        d.StationId,
                        StationName = d.Station.Name,
                        d.VideoUrl,
                        d.Status,
                        d.LatestOnlineTime,
                        d.UpdatedAt
                    })
                    .ToListAsync();

                foreach (var device in highResDevices)
                {
                    landmarks.Add(new LandmarkItem
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Lat = device.Lat,
                        Lng = device.Lng,
                        Type = "highResolution",
                        Status = device.Status ?? "offline",
                        Serial = device.Serial,
                        StationID = device.StationId,
                        StationName = device.StationName,
                        VideoUrl = device.VideoUrl,
                        LatestOnlineTime = device.LatestOnlineTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = device.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return new LandmarksResponse
                {
                    Landmarks = landmarks
                };
            }) ?? new LandmarksResponse { Landmarks = new List<LandmarkItem>() };
        }

        // 保留其他現有方法但簡化
        public async Task<ParkingLandmarkResponse> GetLandmarkParkingAsync(int id)
        {
            var device = await _context.ParkingDevices
                .AsNoTracking()
                .Include(d => d.Station)
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                throw new ArgumentException("找不到指定的停車場設備");
            }

            if (!_authService.HasStationPermission(device.StationId))
            {
                throw new UnauthorizedAccessException("沒有權限存取此分站");
            }

            // 獲取最近7天的最後一筆記錄
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var lastRecord = await _context.ParkingRecords
                .AsNoTracking()
                .Where(r => r.DeviceSerial == device.Serial && r.Time >= sevenDaysAgo)
                .OrderByDescending(r => r.Time)
                .FirstOrDefaultAsync();

            var usedCount = lastRecord?.OccupiedSpaces ?? 0;
            var lastUpdateTime = lastRecord?.Time.ToString("yyyy-MM-dd HH:mm:ss");

            // 獲取過去24小時的歷史數據（每小時一筆）
            var oneDayAgo = DateTime.Now.AddHours(-23);
            var records = await _context.ParkingRecords
                .AsNoTracking()
                .Where(r => r.DeviceSerial == device.Serial && r.Time >= oneDayAgo)
                .OrderByDescending(r => r.Time)
                .ToListAsync();

            // 建立每小時的歷史數據
            var historyDatas = new Dictionary<string, double>();

            // 建立時間間隔（每小時）
            for (var time = oneDayAgo; time <= DateTime.Now; time = time.AddHours(1))
            {
                var timeKey = time.ToString("HH:mm");
                historyDatas[timeKey] = 0;
            }

            // 填入實際數據
            foreach (var record in records)
            {
                // 找到最接近的小時間隔
                var roundedTime = new DateTime(record.CreatedAt.Year, record.CreatedAt.Month, record.CreatedAt.Day,
                    record.CreatedAt.Hour, 0, 0);
                var timeKey = roundedTime.ToString("HH:mm");

                if (historyDatas.ContainsKey(timeKey))
                {
                    historyDatas[timeKey] = device.NumberOfParking > 0
                        ? (double)record.OccupiedSpaces / device.NumberOfParking * 100
                        : 0;
                }
            }

            return new ParkingLandmarkResponse
            {
                Id = device.Id,
                Name = device.Name,
                Lat = device.Lat,
                Lng = device.Lng,
                Serial = device.Serial,
                NumberOfParking = device.NumberOfParking,
                Status = device.Status ?? "unknown",
                LatestRecord = lastRecord != null ? new ParkingRecordInfo
                {
                    Time = ((DateTimeOffset)lastRecord.Time).ToUnixTimeMilliseconds(),
                    ParkedNum = lastRecord.OccupiedSpaces,
                    DeviceSerial = lastRecord.DeviceSerial
                } : null,
                CurrentParked = usedCount,
                AvailableSpaces = device.NumberOfParking - usedCount,
                HistoryDatas = historyDatas,
                LastUpdateTime = lastUpdateTime
            };
        }

        public async Task<TrafficLandmarkResponse> GetLandmarkTrafficAsync(int id)
        {
            var device = await _context.TrafficDevices
                .AsNoTracking()
                .Include(d => d.Station)
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                throw new ArgumentException("找不到指定的交通設備");
            }

            if (!_authService.HasStationPermission(device.StationId))
            {
                throw new UnauthorizedAccessException("沒有權限存取此分站");
            }

            // 獲取最近7天的最後一筆記錄
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var lastRecord = await _context.TrafficRecords
                .AsNoTracking()
                .Where(r => r.DeviceSerial == device.Serial && r.Time >= sevenDaysAgo)
                .OrderByDescending(r => r.Time)
                .FirstOrDefaultAsync();

            var speed = lastRecord?.AverageSpeed ?? 0;
            var lastUpdateTime = lastRecord?.Time.ToString("yyyy-MM-dd HH:mm:ss");

            // 計算壅塞狀態（基於速度和限速）
            var congestionStatus = CalculateTrafficCongestionStatus(speed, device.SpeedLimit);

            // 獲取過去7天的歷史數據
            var records = await _context.TrafficRecords
                .AsNoTracking()
                .Where(r => r.DeviceSerial == device.Serial && r.Time >= sevenDaysAgo)
                .OrderBy(r => r.Time)
                .ToListAsync();

            // 建立歷史數據（每小時一筆）
            var historyDatas = new Dictionary<string, string>();
            foreach (var record in records)
            {
                var timeKey = record.Time.ToString("yyyy-MM-dd HH:mm:ss");
                historyDatas[timeKey] = CalculateTrafficChartCongestionStatus(record.AverageSpeed, device.SpeedLimit);
            }

            return new TrafficLandmarkResponse
            {
                Id = device.Id,
                Name = device.Name,
                Lat = device.Lat,
                Lng = device.Lng,
                Serial = device.Serial,
                City = device.City ?? "",
                ETagNumber = device.ETagNumber ?? "",
                SpeedLimit = device.SpeedLimit,
                Status = device.Status ?? "offline",
                CongestionStatus = congestionStatus,
                LatestRecord = lastRecord != null ? new TrafficRecordInfo
                {
                    Time = ((DateTimeOffset)lastRecord.Time).ToUnixTimeMilliseconds(),
                    SpaceMeanSpeed = lastRecord.AverageSpeed,
                    VehicleCount = lastRecord.VehicleCount,
                    DeviceSerial = lastRecord.DeviceSerial
                } : null,
                CurrentVehicleCount = lastRecord?.VehicleCount ?? 0,
                CurrentAverageSpeed = speed,
                HistoryDatas = historyDatas,
                LastUpdateTime = lastUpdateTime,
                StationName = device.Station?.Name ?? ""
            };
        }

        // 計算交通壅塞狀態的輔助方法
        private string CalculateTrafficCongestionStatus(decimal speed, int speedLimit)
        {
            if (speedLimit <= 0) return "green";

            var ratio = (double)speed / speedLimit;
            if (ratio >= 0.8) return "green";  // 暢通
            if (ratio >= 0.5) return "yellow"; // 稍多
            return "red"; // 壅塞
        }

        private string CalculateTrafficChartCongestionStatus(decimal speed, int speedLimit)
        {
            if (speedLimit <= 0) return "暢通";

            var ratio = (double)speed / speedLimit;
            if (ratio >= 0.8) return "暢通";
            if (ratio >= 0.5) return "稍多";
            return "壅塞";
        }

        public async Task<CrowdLandmarkResponse> GetLandmarkCrowdAsync(int id)
        {
            try
            {
                var device = await _context.CrowdDevices
                    .AsNoTracking()
                    .Include(d => d.Station)
                    .Where(d => d.Id == id)
                    .FirstOrDefaultAsync();

                if (device == null)
                {
                    throw new ArgumentException("找不到指定的人流設備");
                }

                if (!_authService.HasStationPermission(device.StationId))
                {
                    throw new UnauthorizedAccessException("沒有權限存取此分站");
                }

                // 獲取最近7天的最後一筆記錄
                var sevenDaysAgo = DateTime.Now.AddDays(-7);
                var lastRecord = await _context.CrowdRecords
                    .AsNoTracking()
                    .Where(r => r.DeviceSerial == device.Serial && r.Time >= sevenDaysAgo)
                    .OrderByDescending(r => r.Time)
                    .FirstOrDefaultAsync();

                var peopleNum = lastRecord?.Count ?? 0;
                var totalIn = lastRecord?.TotalIn ?? 0;
                var lastUpdateTime = lastRecord?.Time.ToString("yyyy-MM-dd HH:mm:ss");

                // 計算密度和擁擠狀態
                var density = CalculateCrowdDensity(peopleNum, device.Area);
                var congestionStatus = CalculateCrowdCongestionStatus(density);

                // 獲取過去1小時的歷史數據（每5分鐘一筆）
                var oneHourAgo = DateTime.Now.AddMinutes(-60);

                var historyDatas = new Dictionary<string, int>();
                var records = await _context.CrowdRecords
                    .AsNoTracking()
                    .Where(r => r.DeviceSerial == device.Serial && r.Time >= oneHourAgo)
                    .OrderBy(r => r.Time)
                    .ToListAsync();

                // 建立時間間隔（每5分鐘）
                for (var time = oneHourAgo; time <= DateTime.Now; time = time.AddMinutes(5))
                {
                    var timeKey = time.ToString("HH:mm");
                    historyDatas[timeKey] = 0;
                }

                // 填入實際數據
                foreach (var record in records)
                {
                    var timeKey = record.Time.ToString("HH:mm");
                    // 找到最接近的5分鐘間隔
                    var roundedTime = new DateTime(record.Time.Year, record.Time.Month, record.Time.Day,
                        record.Time.Hour, (record.Time.Minute / 5) * 5, 0);
                    var roundedTimeKey = roundedTime.ToString("HH:mm");

                    if (historyDatas.ContainsKey(roundedTimeKey))
                    {
                        historyDatas[roundedTimeKey] = record.Count;
                    }
                }

                return new CrowdLandmarkResponse
                {
                    Id = device.Id,
                    Name = device.Name,
                    Lat = device.Lat,
                    Lng = device.Lng,
                    Serial = device.Serial,
                    Area = device.Area,
                    VideoUrl = device.VideoUrl ?? "",
                    ApiUrl = device.ApiUrl ?? "",
                    Status = device.Status ?? "offline",
                    CongestionStatus = congestionStatus,
                    LatestRecord = lastRecord != null ? new CrowdRecordInfo
                    {
                        Time = ((DateTimeOffset)lastRecord.Time).ToUnixTimeMilliseconds(),
                        Count = lastRecord.Count,
                        TotalIn = lastRecord.TotalIn,
                        DeviceSerial = lastRecord.DeviceSerial
                    } : null,
                    CurrentPeopleCount = peopleNum,
                    CrowdDensity = density,
                    TotalIn = totalIn,
                    HistoryDatas = historyDatas,
                    LastUpdateTime = lastUpdateTime
                };
            }
            catch (Exception ex)
            {
                // Log the error but don't expose internal details
                throw new InvalidOperationException($"無法載入人流設備詳情: {ex.Message}");
            }
        }

        // 計算人流密度的輔助方法
        private double CalculateCrowdDensity(int peopleCount, int area)
        {
            if (area <= 0) return 0;
            return (double)peopleCount / area;
        }

        private string CalculateCrowdCongestionStatus(double density)
        {
            if (density <= 0.1) return "green";  // 空曠
            if (density <= 0.3) return "yellow"; // 稍擠
            return "red"; // 擁擠
        }

        public async Task<FenceLandmarkResponse> GetLandmarkFenceAsync(int id)
        {
            var device = await _context.FenceDevices
                .AsNoTracking()
                .Include(d => d.Station)
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                throw new ArgumentException("找不到指定的圍籬設備");
            }

            if (!_authService.HasStationPermission(device.StationId))
            {
                throw new UnauthorizedAccessException("沒有權限存取此分站");
            }

            // 獲取最近3筆圍籬記錄
            var fenceRecords = await _context.FenceRecords
                .AsNoTracking()
                .Where(r => r.DeviceSerial == device.Serial)
                .OrderByDescending(r => r.Time)
                .Take(3)
                .ToListAsync();

            var events = fenceRecords.Select(record => new FenceEventInfo
            {
                Id = record.Id,
                EventType = (int)record.EventType,
                Time = record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                Image = GetFenceRecordImageUrl(record.Photo) // 需要實作圖片URL處理
            }).ToList();

            // 獲取過去24小時的歷史數據（每小時統計事件數量）
            var oneDayAgo = DateTime.Now.AddHours(-23);

            var historyDatas = new Dictionary<string, int>();

            // 建立時間間隔（每小時）
            for (var time = oneDayAgo; time <= DateTime.Now; time = time.AddHours(1))
            {
                var timeKey = time.ToString("HH:mm");
                historyDatas[timeKey] = 0;
            }

            var records = await _context.FenceRecords
                .AsNoTracking()
                .Where(r => r.DeviceSerial == device.Serial && r.Time >= oneDayAgo)
                .OrderBy(r => r.Time)
                .ToListAsync();

            // 統計每小時的事件數量
            foreach (var record in records)
            {
                // 找到最接近的小時間隔
                var roundedTime = new DateTime(record.Time.Year, record.Time.Month, record.Time.Day,
                    record.Time.Hour, 0, 0);
                var timeKey = roundedTime.ToString("HH:mm");

                if (historyDatas.ContainsKey(timeKey))
                {
                    historyDatas[timeKey]++;
                }
            }

            return new FenceLandmarkResponse
            {
                Id = device.Id,
                Name = device.Name,
                Lat = device.Lat,
                Lng = device.Lng,
                Serial = device.Serial,
                ObservingTime = $"{device.ObservingTimeStart} - {device.ObservingTimeEnd}",
                Status = device.Status ?? "unknown",
                LatestRecord = fenceRecords.FirstOrDefault() != null ? new FenceRecordInfo
                {
                    Time = ((DateTimeOffset)fenceRecords.First().Time).ToUnixTimeMilliseconds(),
                    EventType = (int)fenceRecords.First().EventType,
                    Photo = fenceRecords.First().Photo,
                    DeviceSerial = fenceRecords.First().DeviceSerial
                } : null,
                LastEvent = fenceRecords.FirstOrDefault()?.EventType.ToString(),
                LastEventTime = fenceRecords.FirstOrDefault() != null ?
                    ((DateTimeOffset)fenceRecords.First().Time).ToUnixTimeMilliseconds() : null,
                Events = events,
                HistoryDatas = historyDatas,
                LastUpdateTime = fenceRecords.FirstOrDefault()?.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                StationName = device.Station?.Name ?? ""
            };
        }

        // 處理圍籬記錄圖片URL的輔助方法
        private string GetFenceRecordImageUrl(string? photo)
        {
            if (string.IsNullOrEmpty(photo))
                return "/images/image-placeholder.png";

            // 使用 FileService 來獲取正確的上傳文件 URL
            return _fileService.GetUploadFileUrl("fence-record", photo);
        }

        public async Task<HighResolutionLandmarkResponse> GetLandmarkHighResolutionAsync(int id)
        {
            var device = await _context.HighResolutionDevices
                .AsNoTracking()
                .Include(d => d.Station)
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                throw new ArgumentException("找不到指定的4K影像設備");
            }

            if (!_authService.HasStationPermission(device.StationId))
            {
                throw new UnauthorizedAccessException("沒有權限存取此分站");
            }

            return new HighResolutionLandmarkResponse
            {
                Id = device.Id,
                Name = device.Name,
                Lat = device.Lat,
                Lng = device.Lng,
                Serial = device.Serial,
                VideoUrl = device.VideoUrl ?? "",
                Status = device.Status ?? "unknown",
                LastUpdateTime = device.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        // 新增：公開方法用於取得壅塞/暢通狀態顯示
        public string GetTrafficCongestionDisplayStatus(decimal speed, int speedLimit)
        {
            if (speedLimit <= 0) return "暢通";

            var ratio = (double)speed / speedLimit;
            if (ratio >= 0.8) return "暢通";
            if (ratio >= 0.5) return "稍多";
            return "壅塞";
        }

        public string GetCrowdCongestionDisplayStatus(double density)
        {
            if (density <= 0.1) return "空曠";
            if (density <= 0.3) return "稍擠";
            return "擁擠";
        }
    }
}