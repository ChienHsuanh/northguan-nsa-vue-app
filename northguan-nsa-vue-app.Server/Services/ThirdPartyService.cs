using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using northguan_nsa_vue_app.Server.Controllers;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Services.Infrastructure;
using System.IO;
using System.Text.Json;
using System.Globalization;
using northguan_nsa_vue_app.Server.Exceptions;

namespace northguan_nsa_vue_app.Server.Services
{
    // Placeholder implementations for services that need to be fully implemented later


    public class SystemSettingService : ISystemSettingService
    {
        public async Task<SystemSettingResponse> GetSystemSettingAsync()
        {
            return new SystemSettingResponse { Message = "GetSystemSetting needs implementation" };
        }

        public async Task UpdateSystemSettingAsync(UpdateSystemSettingRequest request)
        {
            // TODO: Implement system setting update logic
            await Task.CompletedTask;
        }
    }

    public class ThirdPartyService : IThirdPartyService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ThirdPartyService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileService _fileService;

        // Common date time formats used by third-party APIs
        private readonly string[] _timeFormats = {
            "yyyy/MM/dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd HH:mm",
            "yyyy-MM-dd HH:mm"
        };

        public ThirdPartyService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<ThirdPartyService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFileService fileService)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
        }

        /// <summary>
        /// Helper method to parse DateTime with multiple supported formats
        /// </summary>
        private DateTime ParseDateTime(string timeString, string methodName)
        {
            if (DateTime.TryParseExact(timeString, _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            _logger.LogError("無法解析時間格式 in {MethodName}：{Time}", methodName, timeString);
            throw new FormatException($"無法解析時間格式：{timeString}");
        }

        public async Task<CreateFenceRecordResponse> CreateFenceRecordAsync(CreateFenceRecordRequest request)
        {
            // 解析字串為 DateTime 物件，但不帶時區資訊
            DateTime unspecifiedTime = ParseDateTime(request.Time, nameof(CreateFenceRecordAsync));

            // 查找台北時區
            var taiwanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

            // 根據台北時區的偏移量創建 DateTimeOffset 物件
            DateTimeOffset taiwanTimeOffset = new(unspecifiedTime, taiwanTimeZone.GetUtcOffset(unspecifiedTime));

            // 將台北時間轉換為 UTC
            DateTimeOffset utcTimeOffset = taiwanTimeOffset.ToUniversalTime();

            var deviceSerial = request.DeviceSerial;
            var eventType = request.EventType;
            var snapshot = request.Snapshot;

            _logger.LogInformation("收到圍籬記錄請求：設備 {DeviceSerial}，事件類型 {EventType}，時間 (UTC) {Time}",
                deviceSerial, eventType, utcTimeOffset);

            // 查找設備
            FenceDevice? device = null;
            if (!string.IsNullOrEmpty(deviceSerial))
            {
                try
                {
                    device = await _context.FenceDevices
                        .Include(d => d.Station)
                        .FirstOrDefaultAsync(d => d.Serial.Contains(deviceSerial));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "無法找到圍籬設備：{DeviceSerial}", deviceSerial);
                    throw new ResourceNotFoundException($"無法找到圍籬設備：{deviceSerial}");
                }
            }

            if (device == null)
            {
                _logger.LogWarning("系統中未有此裝置：{DeviceSerial}", deviceSerial);
                throw new ResourceNotFoundException("系統中未有此裝置");
            }

            // 檢查是否應該發送警報
            bool shouldAlert = await ShouldSendAlertAsync(device, DateTime.Now);

            _logger.LogInformation("圍籬警報檢查結果：{ShouldAlert}，設備：{DeviceSerial}", shouldAlert, deviceSerial);

            if (shouldAlert)
            {
                string? filename = null;
                string? filePath = null;

                // 處理快照圖片
                if (!string.IsNullOrEmpty(snapshot))
                {
                    try
                    {
                        filename = await _fileService.SaveBase64ImageAsync("fence-record", snapshot, ".jpg");

                        // 建立圖片的完整 URL
                        filePath = GetImageUrl(filename);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "儲存圍籬記錄圖片時發生錯誤：{DeviceSerial}", deviceSerial);
                        throw new InternalServerErrorException("儲存圍籬記錄圖片時發生錯誤", ex);
                    }
                }

                // 建立圍籬記錄
                var fenceRecord = new FenceRecord
                {
                    DeviceSerial = deviceSerial,
                    EventType = (FenceEventType)eventType,
                    Photo = filename,
                    Time = utcTimeOffset.DateTime
                };

                _context.FenceRecords.Add(fenceRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("已建立圍籬記錄：設備 {DeviceSerial}，事件類型 {EventType}", deviceSerial, eventType);

                // 發送 LINE 通知
                await SendFenceNotificationAsync(device, eventType, filePath);
            }

            // 更新設備狀態
            device.Status = "online";
            device.LatestOnlineTime = utcTimeOffset.DateTime;
            await _context.SaveChangesAsync();

            return new CreateFenceRecordResponse
            {
                Message = "ok"
            };
        }

        /// <summary>
        /// 檢查是否應該發送警報（基於觀察時間窗口）
        /// </summary>
        private async Task<bool> ShouldSendAlertAsync(FenceDevice device, DateTime eventTime)
        {
            try
            {
                var observingTimeStart = device.ObservingTimeStart;
                var observingTimeEnd = device.ObservingTimeEnd;

                if (string.IsNullOrEmpty(observingTimeStart) || string.IsNullOrEmpty(observingTimeEnd))
                {
                    return true; // 如果沒有設定觀察時間，則總是發送警報
                }

                var eventTimeInt = int.Parse(eventTime.ToString("HHmm"));
                var startTimeInt = int.Parse(observingTimeStart.Replace(":", ""));
                var endTimeInt = int.Parse(observingTimeEnd.Replace(":", ""));

                _logger.LogDebug("圍籬時間檢查：開始 {Start}，結束 {End}，事件時間 {EventTime}",
                    startTimeInt, endTimeInt, eventTimeInt);

                bool shouldAlert;
                if (endTimeInt < startTimeInt) // 跨日情況（例如 22:00 到 06:00）
                {
                    shouldAlert = !(endTimeInt < eventTimeInt && eventTimeInt < startTimeInt);
                }
                else // 同日情況（例如 08:00 到 18:00）
                {
                    shouldAlert = !(endTimeInt < eventTimeInt || startTimeInt > eventTimeInt);
                }

                _logger.LogDebug("圍籬警報決定：{ShouldAlert}", shouldAlert);
                return shouldAlert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查圍籬觀察時間時發生錯誤，設備：{DeviceSerial}", device.Serial);
                return true; // 發生錯誤時預設發送警報
            }
        }

        /// <summary>
        /// 發送圍籬通知
        /// </summary>
        private async Task SendFenceNotificationAsync(FenceDevice device, int eventType, string? filePath)
        {
            try
            {
                var station = device.Station;
                if (station != null && !string.IsNullOrEmpty(station.LineToken) && station.EnableNotify)
                {
                    var eventDescription = ((FenceEventType)eventType).GetDescription();
                    var message = $"{station.Name} {device.Name} {eventDescription}";

                    await _notificationService.SendLineNotificationAsync(station.LineToken, message, filePath);

                    _logger.LogInformation("已發送圍籬通知：站點 {StationName}，設備 {DeviceName}，事件 {EventType}",
                        station.Name, device.Name, eventDescription);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送圍籬通知時發生錯誤：設備 {DeviceSerial}", device.Serial);
            }
        }


        /// <summary>
        /// 取得圖片的完整 URL
        /// </summary>
        private string GetImageUrl(string filename)
        {
            try
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    var scheme = request.Scheme;
                    var host = request.Host.Value;
                    return $"{scheme}://{host}{_fileService.GetUploadFileUrl("fence-record", filename)}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立圖片 URL 時發生錯誤：{Filename}", filename);
            }

            return _fileService.GetUploadFileUrl("fence-record", filename); // 回退到相對路徑
        }

        public async Task<UpdateFenceHeartbeatResponse> UpdateFenceHeartbeatAsync(UpdateFenceHeartbeatRequest request)
        {
            DateTime updateTime = ParseDateTime(request.Time, nameof(UpdateFenceHeartbeatAsync));

            var device = await _context.FenceDevices.FirstOrDefaultAsync(d => d.Serial == request.DeviceSerial);

            if (device == null)
            {
                _logger.LogWarning("系統中未有此裝置：{DeviceSerial}", request.DeviceSerial);
                throw new ResourceNotFoundException("系統中未有此裝置");
            }

            device.Status = "online";
            device.LatestOnlineTime = updateTime;
            await _context.SaveChangesAsync();

            // Log device status
            var statusLog = new DeviceStatusLog
            {
                DeviceType = "fence",
                DeviceSerial = request.DeviceSerial,
                Status = "heartbeat",
                Timestamp = updateTime
            };
            _context.DeviceStatusLogs.Add(statusLog);
            await _context.SaveChangesAsync();

            return new UpdateFenceHeartbeatResponse
            {
                Message = "ok"
            };
        }

                public async Task<CreateCrowdRecordResponse> CreateCrowdRecordAsync(CreateCrowdRecordRequest request)
                {
                    DateTime recordTime = ParseDateTime(request.Time, nameof(CreateCrowdRecordAsync));
        
                    var crowdRecord = new CrowdRecord
                    {
                        DeviceSerial = request.DeviceSerial,
                        Count = request.PeopleCount,
                        Time = recordTime
                    };
        
                    _context.CrowdRecords.Add(crowdRecord);
                    await _context.SaveChangesAsync();
        
                    // Update device status
                    var device = await _context.CrowdDevices.FirstOrDefaultAsync(d => d.Serial == request.DeviceSerial);
                    if (device == null)
                    {
                        _logger.LogWarning("系統中未有此裝置：{DeviceSerial}", request.DeviceSerial);
                        throw new ResourceNotFoundException("系統中未有此裝置");
                    }
        
                    device.Status = "online";
                    device.LatestOnlineTime = recordTime;
                    await _context.SaveChangesAsync();
        
                    return new CreateCrowdRecordResponse
                    {
                        Message = "ok"
                    };
                }
        public async Task<CreateParkingRecordResponse> CreateParkingRecordAsync(CreateParkingRecordRequest request)
        {
            // Only process CarType 'C' records as per old project logic
            if (request.CarType != "C")
            {
                return new CreateParkingRecordResponse
                {
                    Message = "ok"
                };
            }

            DateTime recordTime = ParseDateTime(request.Time, nameof(CreateParkingRecordAsync));

            var parkingRecord = new ParkingRecord
            {
                DeviceSerial = request.DeviceSerial,
                OccupiedSpaces = request.ParkedNum,
                Time = recordTime,
                TotalSpaces = 0,     // 第三方API未提供，設為0
                AvailableSpaces = 0, // 第三方API未提供，設為0
                OccupancyRate = 0    // 第三方API未提供，設為0
            };

            _context.ParkingRecords.Add(parkingRecord);
            await _context.SaveChangesAsync();

            // Update device status
            var device = await _context.ParkingDevices.FirstOrDefaultAsync(d => d.Serial == request.DeviceSerial);
            if (device == null)
            {
                _logger.LogWarning("系統中未有此裝置：{DeviceSerial}", request.DeviceSerial);
                throw new ResourceNotFoundException("系統中未有此裝置");
            }

            device.Status = "online";
            device.LatestOnlineTime = recordTime;
            await _context.SaveChangesAsync();

            return new CreateParkingRecordResponse
            {
                Message = "ok"
            };
        }

        public async Task<List<ThirdPartyStationInfo>> GetStationListAsync()
        {
            return await _context.Stations
                .Select(s => new ThirdPartyStationInfo
                {
                    Id = s.Id,
                    Name = s.Name,
                    Lat = s.Lat,
                    Lng = s.Lng
                })
                .ToListAsync();
        }

        public async Task<List<ThirdPartyFenceDeviceInfo>> GetFenceDeviceListAsync()
        {
            return await _context.FenceDevices
                .Join(_context.Stations, d => d.StationId, s => s.Id, (d, s) => new { Device = d, Station = s })
                .Where(x => x.Station.DeletedAt == null)
                .Select(x => new ThirdPartyFenceDeviceInfo
                {
                    Id = x.Device.Id,
                    Name = x.Device.Name,
                    StationId = x.Device.StationId
                })
                .ToListAsync();
        }

        public async Task<List<ThirdPartyCrowdDeviceInfo>> GetCrowdDeviceListAsync()
        {
            // Get latest crowd record IDs (similar to old project logic)
            var latestRecordIds = await _context.CrowdRecords
                .GroupBy(r => r.DeviceSerial)
                .Select(g => g.Max(r => r.Id))
                .ToListAsync();

            var devices = await _context.CrowdDevices
                .Join(_context.Stations, d => d.StationId, s => s.Id, (d, s) => new { Device = d, Station = s })
                .Where(x => x.Station.DeletedAt == null)
                .GroupJoin(_context.CrowdRecords.Where(r => latestRecordIds.Contains(r.Id)),
                    x => x.Device.Serial,
                    r => r.DeviceSerial,
                    (x, records) => new { x.Device, x.Station, Record = records.FirstOrDefault() })
                .Select(x => new ThirdPartyCrowdDeviceInfo
                {
                    Id = x.Device.Serial, // Use serial as id like old project
                    Name = x.Device.Name,
                    StationId = x.Device.StationId,
                    Lat = x.Device.Lat,
                    Lng = x.Device.Lng,
                    Area = x.Device.Area,
                    Count = x.Record != null ? x.Record.Count : null,
                    Time = x.Record != null ? x.Record.UpdatedAt.ToString("yyyy/MM/dd\\THH:mm:ss") : null
                })
                .ToListAsync();

            return devices;
        }

        public async Task<List<ThirdPartyParkingDeviceInfo>> GetParkingDeviceListAsync()
        {
            // Get latest parking record IDs (similar to old project logic)
            var latestRecordIds = await _context.ParkingRecords
                .GroupBy(r => r.DeviceSerial)
                .Select(g => g.Max(r => r.Id))
                .ToListAsync();

            var devices = await _context.ParkingDevices
                .Join(_context.Stations, d => d.StationId, s => s.Id, (d, s) => new { Device = d, Station = s })
                .Where(x => x.Station.DeletedAt == null)
                .GroupJoin(_context.ParkingRecords.Where(r => latestRecordIds.Contains(r.Id)),
                    x => x.Device.Serial,
                    r => r.DeviceSerial,
                    (x, records) => new { x.Device, x.Station, Record = records.FirstOrDefault() })
                .Select(x => new ThirdPartyParkingDeviceInfo
                {
                    Id = x.Device.Serial, // Use serial as id like old project
                    Name = x.Device.Name,
                    Lat = x.Device.Lat,
                    Lng = x.Device.Lng,
                    StationId = x.Device.StationId,
                    NumberOfSpaces = x.Device.NumberOfParking,
                    ParkedNum = x.Record != null ? x.Record.OccupiedSpaces : 0,
                    TotalIn = 0,  // 舊專案固定回傳 0
                    TotalOut = 0, // 舊專案固定回傳 0
                    Time = x.Record != null ? x.Record.Time.ToString("yyyy/MM/dd\\THH:mm:ss") : null
                })
                .ToListAsync();

            return devices;
        }

        public async Task<List<ThirdPartyTrafficDeviceInfo>> GetTrafficDeviceListAsync()
        {
            // Get latest traffic record IDs (similar to old project logic)
            var latestRecordIds = await _context.TrafficRecords
                .GroupBy(r => r.DeviceSerial)
                .Select(g => g.Max(r => r.Id))
                .ToListAsync();

            var devices = await _context.TrafficDevices
                .Join(_context.Stations, d => d.StationId, s => s.Id, (d, s) => new { Device = d, Station = s })
                .Where(x => x.Station.DeletedAt == null)
                .GroupJoin(_context.TrafficRecords.Where(r => latestRecordIds.Contains(r.Id)),
                    x => x.Device.Serial,
                    r => r.DeviceSerial,
                    (x, records) => new { x.Device, x.Station, Record = records.FirstOrDefault() })
                .Select(x => new ThirdPartyTrafficDeviceInfo
                {
                    Id = x.Device.Serial, // Use serial as id like old project
                    Name = x.Device.Name,
                    Lat = x.Device.Lat,
                    Lng = x.Device.Lng,
                    StationId = x.Device.StationId,
                    EtagPairId = x.Device.ETagNumber,
                    SpeedLimit = x.Device.SpeedLimit,
                    TravelTime = x.Record != null ? x.Record.TravelTime : null,
                    SpaceMeanSpeed = x.Record != null ? x.Record.AverageSpeed : null,
                    Time = x.Record != null ? x.Record.Time.ToString("yyyy/MM/dd\\THH:mm:ss") : null
                })
                .ToListAsync();

            return devices;
        }

        public async Task<byte[]> CreateZeroTouchExcelAsync()
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("無接觸資料");

            // Headers
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("時間");
            headerRow.CreateCell(1).SetCellValue("分站");
            headerRow.CreateCell(2).SetCellValue("事件類型");
            headerRow.CreateCell(3).SetCellValue("詳細資訊");

            // Sample data - you can implement actual zero touch logic here
            IRow dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dataRow.CreateCell(1).SetCellValue("系統");
            dataRow.CreateCell(2).SetCellValue("無接觸檔案產生");
            dataRow.CreateCell(3).SetCellValue("檔案產生成功");

            // Auto-fit columns
            for (int i = 0; i < 4; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream, true);
                return memoryStream.ToArray();
            }
        }

        public async Task<List<FenceDeviceConfigDto>> GetFenceDeviceConfigsAsync()
        {
            var devices = await _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => d.DeletedAt == null && d.Station.DeletedAt == null)
                .ToListAsync();

            return devices.Select(d =>
            {
                var dto = new FenceDeviceConfigDto
                {
                    Serial = d.Serial,
                    Name = d.Name,
                    VideoUrl = d.VideoUrl,
                    ObservingTimeStart = d.ObservingTimeStart,
                    ObservingTimeEnd = d.ObservingTimeEnd
                };

                if (!string.IsNullOrEmpty(d.Zones))
                {
                    try
                    {
                        dto.Zones = JsonSerializer.Deserialize<List<FenceZoneDto>>(d.Zones) ?? new();
                    }
                    catch
                    {
                        dto.Zones = new();
                    }
                }

                if (!string.IsNullOrEmpty(d.CameraConfig))
                {
                    try
                    {
                        dto.CameraConfig = JsonSerializer.Deserialize<JsonElement>(d.CameraConfig);
                    }
                    catch
                    {
                        dto.CameraConfig = null;
                    }
                }

                return dto;
            }).ToList();
        }

        public async Task<UpdateFenceZonesResponse> UpdateFenceZonesAsync(UpdateFenceZonesRequest request)
        {
            var device = await _context.FenceDevices
                .FirstOrDefaultAsync(d => d.Serial == request.DeviceSerial && d.DeletedAt == null);

            if (device == null)
                throw new KeyNotFoundException($"FenceDevice with serial '{request.DeviceSerial}' not found");

            device.Zones = JsonSerializer.Serialize(request.Zones);
            device.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return new UpdateFenceZonesResponse
            {
                Message = "ok",
                Count = request.Zones.Count
            };
        }
    }
}