using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class FenceRecordService : IFenceRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public FenceRecordService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<PagedResponse<FenceRecordListResponse>> GetRecordsListAsync(FenceRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.FenceRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply event type filters
            if (parameters.EventTypes != null && parameters.EventTypes.Count > 0)
            {
                var eventTypeEnums = parameters.EventTypes.Select(et => Enum.Parse<FenceEventType>(et)).ToList();
                query = query.Where(r => eventTypeEnums.Contains(r.EventType));
            }

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
                    eventType = r.EventType,
                    photo = r.Photo,
                    time = r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    timestamp = ((DateTimeOffset)r.Time).ToUnixTimeSeconds()
                })
                .ToListAsync();

            // Get device and station info
            var devicesInfo = await _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new { d.Serial, d.Name, StationName = d.Station!.Name })
                .AsNoTracking()
                .ToListAsync();

            var data = records.Select(r => new FenceRecordListResponse
            {
                Id = r.id,
                DeviceSerial = r.deviceSerial,
                DeviceName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.Name ?? "未知裝置",
                StationName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.StationName ?? "未知分站",
                Event = r.eventType.GetDescription(),
                ImageUrl = GetFenceRecordImageUrl(r.photo),
                Time = r.time,
                Timestamp = r.timestamp
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.Size);

            return new PagedResponse<FenceRecordListResponse>
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

            return await _context.FenceRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .CountAsync();
        }

        private record FenceDeviceInfo(string Serial, string Name, string StationName);

        public async IAsyncEnumerable<FenceRecordListResponse> StreamAllFenceRecordsAsync(FenceRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.FenceRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply event type filters
            if (parameters.EventTypes != null && parameters.EventTypes.Count > 0)
            {
                var eventTypeEnums = parameters.EventTypes.Select(et => Enum.Parse<FenceEventType>(et)).ToList();
                query = query.Where(r => eventTypeEnums.Contains(r.EventType));
            }

            // Apply sorting
            query = parameters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.Time)
                : query.OrderByDescending(r => r.Time);

            // Fetch device and station info once
            var devicesInfo = await _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new FenceDeviceInfo(d.Serial, d.Name, d.Station!.Name))
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Serial!, d => d);

            await foreach (var record in query.AsAsyncEnumerable())
            {
                FenceDeviceInfo? deviceInfo = null;
                devicesInfo.TryGetValue(record.DeviceSerial, out deviceInfo);

                yield return new FenceRecordListResponse
                {
                    Id = record.Id,
                    DeviceSerial = record.DeviceSerial,
                    DeviceName = deviceInfo?.Name ?? "未知裝置",
                    StationName = deviceInfo?.StationName ?? "未知分站",
                    Event = record.EventType.GetDescription(),
                    ImageUrl = GetFenceRecordImageUrl(record.Photo),
                    Time = record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)record.Time).ToUnixTimeSeconds()
                };
            }
        }

        private async Task<List<string>> GetAvailableDeviceSerialsAsync(List<int> availableStationIds, string keyword)
        {
            var query = _context.FenceDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) && d.Station.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.Name.Contains(keyword) || d.Serial.Contains(keyword));
            }

            return await query.Select(d => d.Serial).ToListAsync();
        }

        private string GetFenceRecordImageUrl(string? photo)
        {
            if (string.IsNullOrEmpty(photo))
                return "/images/image-placeholder.png";

            // 使用 FileService 來獲取正確的上傳文件 URL
            return _fileService.GetUploadFileUrl("fence-record", photo);
        }
    }
}