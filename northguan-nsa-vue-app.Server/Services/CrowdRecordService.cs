using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class CrowdRecordService : ICrowdRecordService
    {
        private readonly ApplicationDbContext _context;

        public CrowdRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<CrowdRecordListResponse>> GetRecordsListAsync(CrowdRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.CrowdRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply people count filters
            if (parameters.MinPeopleCount.HasValue)
                query = query.Where(r => r.Count >= parameters.MinPeopleCount.Value);
            if (parameters.MaxPeopleCount.HasValue)
                query = query.Where(r => r.Count <= parameters.MaxPeopleCount.Value);

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
                    peopleCount = r.Count,
                    time = r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    timestamp = ((DateTimeOffset)r.Time).ToUnixTimeSeconds()
                })
                .ToListAsync();

            // Get device and station info
            var devicesInfo = await _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new { d.Serial, d.Name, d.Area, StationName = d.Station!.Name })
                .AsNoTracking()
                .ToListAsync();

            var data = records.Select(r => new CrowdRecordListResponse
            {
                Id = r.id,
                DeviceSerial = r.deviceSerial,
                DeviceName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.Name ?? "未知裝置",
                StationName = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.StationName ?? "未知分站",
                Area = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.Area ?? 0,
                PeopleCount = r.peopleCount,
                Density = devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial)?.Area > 0 ?
                    r.peopleCount / (double)devicesInfo.FirstOrDefault(d => d.Serial == r.deviceSerial).Area : 0,
                Time = r.time,
                Timestamp = r.timestamp
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.Size);

            return new PagedResponse<CrowdRecordListResponse>
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

        private record DeviceInfo(string Serial, string Name, int Area, string StationName);

        public async IAsyncEnumerable<CrowdRecordListResponse> StreamAllCrowdRecordsAsync(CrowdRecordQueryParameters parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);

            var query = _context.CrowdRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .AsNoTracking();

            // Apply date filters
            if (parameters.StartDate.HasValue)
                query = query.Where(r => r.Time >= parameters.StartDate.Value);
            if (parameters.EndDate.HasValue)
                query = query.Where(r => r.Time <= parameters.EndDate.Value);

            // Apply people count filters
            if (parameters.MinPeopleCount.HasValue)
                query = query.Where(r => r.Count >= parameters.MinPeopleCount.Value);
            if (parameters.MaxPeopleCount.HasValue)
                query = query.Where(r => r.Count <= parameters.MaxPeopleCount.Value);

            // Apply sorting (optional for streaming, but good for consistent order)
            query = parameters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.Time)
                : query.OrderByDescending(r => r.Time);

            // Fetch device and station info once
            var devicesInfo = await _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .Select(d => new DeviceInfo(d.Serial, d.Name, d.Area, d.Station!.Name))
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Serial!, d => d);

            await foreach (var record in query.AsAsyncEnumerable())
            {
                DeviceInfo? deviceInfo = null;
                devicesInfo.TryGetValue(record.DeviceSerial, out deviceInfo);

                yield return new CrowdRecordListResponse
                {
                    Id = record.Id,
                    DeviceSerial = record.DeviceSerial,
                    DeviceName = deviceInfo?.Name ?? "未知裝置",
                    StationName = deviceInfo?.StationName ?? "未知分站",
                    Area = deviceInfo?.Area ?? 0,
                    PeopleCount = record.Count,
                    Density = (deviceInfo?.Area > 0 && deviceInfo.Area != null) ?
                        record.Count / (double)deviceInfo.Area : 0,
                    Time = record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = ((DateTimeOffset)record.Time).ToUnixTimeSeconds()
                };
            }
        }

        public async Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(availableStationIds, keyword);

            return await _context.CrowdRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .CountAsync();
        }

        public async Task<byte[]> ExportRecordsAsync(string keyword, List<int> availableStationIds)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(availableStationIds, keyword);

            var records = await _context.CrowdRecords
                .Where(r => deviceSerials.Contains(r.DeviceSerial))
                .OrderByDescending(r => r.Time)
                .AsNoTracking()
                .ToListAsync();

            // Get device and station info
            var devicesInfo = await _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => deviceSerials.Contains(d.Serial))
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Serial!, d => new { d.Name, d.Area, StationName = d.Station!.Name });

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("人流記錄");

            // Headers
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("分站名稱");
            headerRow.CreateCell(1).SetCellValue("裝置名稱");
            headerRow.CreateCell(2).SetCellValue("裝置序號");
            headerRow.CreateCell(3).SetCellValue("人數");
            headerRow.CreateCell(4).SetCellValue("區域面積");
            headerRow.CreateCell(5).SetCellValue("人流密度");
            headerRow.CreateCell(6).SetCellValue("時間");

            // Data
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var deviceInfo = devicesInfo.GetValueOrDefault(record.DeviceSerial);
                IRow dataRow = sheet.CreateRow(i + 1);

                dataRow.CreateCell(0).SetCellValue(deviceInfo?.StationName ?? "未知分站");
                dataRow.CreateCell(1).SetCellValue(deviceInfo?.Name ?? "未知裝置");
                dataRow.CreateCell(2).SetCellValue(record.DeviceSerial);
                dataRow.CreateCell(3).SetCellValue(record.Count);
                dataRow.CreateCell(4).SetCellValue(deviceInfo?.Area ?? 0);
                var density = deviceInfo?.Area > 0 ? record.Count / (double)deviceInfo.Area : 0;
                dataRow.CreateCell(5).SetCellValue(density);
                dataRow.CreateCell(6).SetCellValue(record.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            // Auto-fit columns
            for (int i = 0; i < 7; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream, true);
                return memoryStream.ToArray();
            }
        }

        private async Task<List<string>> GetAvailableDeviceSerialsAsync(List<int> availableStationIds, string keyword)
        {
            var query = _context.CrowdDevices
                .Include(d => d.Station)
                .Where(d => availableStationIds.Contains(d.StationId) && d.Station.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.Name.Contains(keyword) || d.Serial.Contains(keyword));
            }

            return await query.Select(d => d.Serial).ToListAsync();
        }
    }
}