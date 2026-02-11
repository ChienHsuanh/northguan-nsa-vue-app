using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 備份服務
    /// </summary>
    public class BackupService
    {
        private readonly IFileManagementService _fileManagement;
        private readonly ICacheService _cache;
        private readonly ILogger<BackupService> _logger;

        public BackupService(
            IFileManagementService fileManagement,
            ICacheService cache,
            ILogger<BackupService> logger)
        {
            _fileManagement = fileManagement;
            _cache = cache;
            _logger = logger;
        }

        public async Task<DatabaseBackupResponse> BackupDatabaseAsync(bool forceExecution = false)
        {
            var response = new DatabaseBackupResponse
            {
                Success = false,
                Message = "資料庫備份失敗"
            };

            try
            {
                var startTime = DateTime.Now;
                var currentDay = DateTime.Now.DayOfWeek;

                // 只在星期一執行備份 (除非強制執行)
                if (!forceExecution && currentDay != DayOfWeek.Monday)
                {
                    _logger.LogDebug("今天不是星期一，跳過資料庫備份");
                    response.Success = true;
                    response.Message = "今天不是星期一，跳過資料庫備份";
                    return response;
                }

                _logger.LogInformation("開始備份資料庫");
                var (filePath, fileSize) = await _fileManagement.BackupDatabaseAsync();
                var endTime = DateTime.Now;

                response.Success = true;
                response.Message = "資料庫備份已完成";
                response.BackupFilePath = filePath;
                response.BackupFileSize = fileSize;
                response.BackupDuration = endTime - startTime;

                _logger.LogInformation("資料庫備份完成");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "備份資料庫時發生錯誤");
                response.Message = $"備份資料庫時發生錯誤: {ex.Message}";
                return response;
            }
        }

        public async Task<LogBackupResponse> BackupWarningLogAsync(bool forceExecution = false)
        {
            var response = new LogBackupResponse
            {
                Success = false,
                Message = "日誌備份失敗"
            };

            try
            {
                var currentMinute = DateTime.Now.Minute;
                var cacheKey = nameof(BackupWarningLogAsync);

                // 只在每小時的前10分鐘執行 (除非強制執行)
                if (!forceExecution && currentMinute >= 10)
                {
                    response.Success = true;
                    response.Message = "不在執行時間範圍內，跳過日誌備份";
                    return response;
                }

                // 檢查是否已經執行過 (除非強制執行)
                if (!forceExecution && await _cache.ExistsAsync(cacheKey))
                {
                    response.Success = true;
                    response.Message = "本小時已執行過，跳過日誌備份";
                    return response;
                }

                _logger.LogInformation("開始備份警告日誌");
                var (backedUpFiles, totalSize, backupLocation) = await _fileManagement.BackupLogFilesAsync();

                // 設置快取，防止重複執行
                await _cache.SetAsync(cacheKey, DateTime.Now, TimeSpan.FromHours(1));

                response.Success = true;
                response.Message = "日誌備份已完成";
                response.BackedUpFiles = backedUpFiles;
                response.TotalBackupSize = totalSize;
                response.BackupLocation = backupLocation;

                _logger.LogInformation("警告日誌備份完成");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "備份警告日誌時發生錯誤");
                response.Message = $"備份警告日誌時發生錯誤: {ex.Message}";
                return response;
            }
        }

        public async Task<AuditLogCleanupResponse> CheckAuditLogAsync(bool forceExecution = false)
        {
            var response = new AuditLogCleanupResponse
            {
                Success = false,
                Message = "審計日誌清理失敗"
            };

            try
            {
                var currentDay = DateTime.Now.Day;
                var cacheKey = nameof(CheckAuditLogAsync);

                // 只在每月1號執行
                if (!forceExecution && currentDay != 1)
                {
                    _logger.LogDebug("今天不是每月1號，跳過審計日誌檢查");
                    response.Success = true;
                    response.Message = "今天不是每月1號，跳過審計日誌檢查";
                    return response;
                }

                // 檢查是否已經執行過
                if (!forceExecution && await _cache.ExistsAsync(cacheKey))
                {
                    response.Success = true;
                    response.Message = "本月已執行過，跳過審計日誌檢查";
                    return response;
                }

                _logger.LogInformation("開始檢查審計日誌");
                var (cleanedFiles, freedSpace) = await _fileManagement.CleanupOldAuditLogsAsync();

                // 設置快取，防止重複執行
                await _cache.SetAsync(cacheKey, DateTime.Now, TimeSpan.FromDays(30));

                response.Success = true;
                response.Message = "審計日誌清理已完成";
                response.CleanedFiles = cleanedFiles;
                response.FreedSpace = (int)freedSpace;
                response.RetentionDays = 30;

                _logger.LogInformation("審計日誌檢查完成");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查審計日誌時發生錯誤");
                response.Message = $"檢查審計日誌時發生錯誤: {ex.Message}";
                return response;
            }
        }

        public async Task SyncZeroTouchVisitorAsync(bool forceExecution = false)
        {
            try
            {
                var currentHour = DateTime.Now.Hour;
                var cacheKey = nameof(SyncZeroTouchVisitorAsync);

                // 只在晚上8點執行
                if (!forceExecution && currentHour != 20)
                {
                    return;
                }

                // 檢查是否已經執行過
                if (!forceExecution && await _cache.ExistsAsync(cacheKey))
                {
                    return;
                }

                _logger.LogInformation("開始同步零接觸訪客數據");
                await _fileManagement.GenerateZeroTouchExcelAsync();

                // 設置快取，防止重複執行
                await _cache.SetAsync(cacheKey, DateTime.Now, TimeSpan.FromHours(23));

                _logger.LogInformation("零接觸訪客數據同步完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "同步零接觸訪客數據時發生錯誤");
                throw;
            }
        }
    }
}