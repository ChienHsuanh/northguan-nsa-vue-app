using Microsoft.Data.SqlClient;
using northguan_nsa_vue_app.Server.DTOs;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    public class FileManagementService : IFileManagementService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileManagementService> _logger;
        private readonly string _backupPath;
        private readonly string _serilogLogPath;

        public FileManagementService(IConfiguration configuration, ILogger<FileManagementService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _backupPath = _configuration.GetValue<string>("BackupSettings:BackupPath") ?? "Backups";

            // 從 Serilog 配置中動態獲取日誌路徑
            _serilogLogPath = GetSerilogLogPath();

            // 確保目錄存在
            Directory.CreateDirectory(_backupPath);
            Directory.CreateDirectory(_serilogLogPath);
        }

        private string GetSerilogLogPath()
        {
            try
            {
                // 從 Serilog 配置中讀取 File sink 的路徑
                var serilogSection = _configuration.GetSection("Serilog:WriteTo");

                foreach (var writeToSection in serilogSection.GetChildren())
                {
                    var sinkName = writeToSection.GetValue<string>("Name");
                    if (sinkName == "File")
                    {
                        var filePath = writeToSection.GetValue<string>("Args:path");
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            // 提取目錄路徑（移除檔名部分）
                            var directoryPath = Path.GetDirectoryName(filePath);
                            if (!string.IsNullOrEmpty(directoryPath))
                            {
                                return directoryPath;
                            }
                        }
                    }
                }

                // 如果無法從配置中讀取，使用預設值
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                return isDevelopment ? "C:\\Logs" : "logs";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "無法從 Serilog 配置中讀取日誌路徑，使用預設值");
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                return isDevelopment ? "C:\\Logs" : "logs";
            }
        }

        public async Task<(string filePath, long fileSize)> BackupDatabaseAsync()
        {
            try
            {
                var currentDay = DateTime.Now.DayOfWeek;

                _logger.LogInformation("開始執行資料庫備份");

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("資料庫連接字串未設定");
                }

                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                var serverName = builder.DataSource;

                // 生成唯一的檔案名稱，避免與現有檔案衝突
                var baseFileName = $"northguan_{DateTime.Now:yyyy-MM-dd_HHmmss}";
                var backupFileName = $"{baseFileName}.bak";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);
                var compressedFilePath = backupFilePath + ".gz";

                // 檢查檔案名稱衝突，如果存在同名的 .bak 或 .bak.gz 檔案，就生成新的唯一名稱
                var counter = 1;
                while (File.Exists(backupFilePath) || File.Exists(compressedFilePath))
                {
                    backupFileName = $"{baseFileName}_{counter:D3}.bak";
                    backupFilePath = Path.Combine(_backupPath, backupFileName);
                    compressedFilePath = backupFilePath + ".gz";
                    counter++;
                    
                    // 防止無限迴圈
                    if (counter > 999)
                    {
                        var uniqueId = Guid.NewGuid().ToString("N")[..8];
                        backupFileName = $"{baseFileName}_{uniqueId}.bak";
                        backupFilePath = Path.Combine(_backupPath, backupFileName);
                        break;
                    }
                }

                _logger.LogInformation("使用備份檔案名稱：{BackupFileName}", backupFileName);

                // 建立 SQL 備份命令
                var backupSql = $@"
                    BACKUP DATABASE [{databaseName}]
                    TO DISK = '{backupFilePath}'
                    WITH FORMAT, INIT,
                    NAME = 'Northguan Database Backup',
                    SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(backupSql, connection);
                command.CommandTimeout = 3600; // 1 小時超時

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("資料庫備份完成，檔案路徑：{BackupFilePath}", backupFilePath);

                // 壓縮備份檔案
                var finalCompressedFilePath = await CompressBackupFileAsync(backupFilePath);

                // 獲取壓縮後的檔案大小
                var fileInfo = new FileInfo(finalCompressedFilePath);
                var fileSize = fileInfo.Exists ? fileInfo.Length : 0;

                // 清理舊的備份檔案
                await CleanupOldDatabaseBackupsAsync();

                return (finalCompressedFilePath, fileSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "資料庫備份失敗");
                throw;
            }
        }

        public async Task<(int backedUpFiles, long totalSize, string backupLocation)> BackupLogFilesAsync()
        {
            try
            {
                _logger.LogInformation("開始備份 Serilog 日誌檔案");

                // 動態獲取當前的 Serilog 日誌檔案
                var currentLogFile = GetCurrentSerilogLogFile();

                if (string.IsNullOrEmpty(currentLogFile) || !File.Exists(currentLogFile))
                {
                    _logger.LogWarning("當前 Serilog 日誌檔案不存在：{LogFile}", currentLogFile);
                    return (0, 0, _serilogLogPath);
                }

                // 創建備份目錄
                var backupPath = Path.Combine(_backupPath, "Logs");
                Directory.CreateDirectory(backupPath);

                var backupFileName = $"serilog-backup-{DateTime.Now:yyyyMMddHHmmss}.log";
                var backupFilePath = Path.Combine(backupPath, backupFileName);

                // 複製日誌檔案到備份目錄
                File.Copy(currentLogFile, backupFilePath, true);

                // 獲取備份檔案大小
                var fileInfo = new FileInfo(backupFilePath);
                var fileSize = fileInfo.Exists ? fileInfo.Length : 0;

                // 使用 Serilog 記錄備份完成
                Log.Information("Serilog 日誌檔案備份完成：{BackupFile}，大小：{FileSize} bytes", backupFilePath, fileSize);

                // 清理舊的備份檔案
                await CleanupOldLogBackupsAsync();

                return (1, fileSize, backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "備份 Serilog 日誌檔案失敗");
                Log.Error(ex, "備份 Serilog 日誌檔案失敗");
                throw;
            }
        }

        private string GetCurrentSerilogLogFile()
        {
            try
            {
                // 從 Serilog 配置中讀取 File sink 的路徑模式
                var serilogSection = _configuration.GetSection("Serilog:WriteTo");

                foreach (var writeToSection in serilogSection.GetChildren())
                {
                    var sinkName = writeToSection.GetValue<string>("Name");
                    if (sinkName == "File")
                    {
                        var filePathPattern = writeToSection.GetValue<string>("Args:path");
                        if (!string.IsNullOrEmpty(filePathPattern))
                        {
                            // 檢查 Serilog 的 rollingInterval 設定
                            var rollingInterval = writeToSection.GetValue<string>("Args:rollingInterval");

                            if (rollingInterval == "Day")
                            {
                                // 日誌按日輪替，需要加上日期
                                var currentDate = DateTime.Now.ToString("yyyyMMdd");
                                var actualFilePath = filePathPattern.Replace("-.", $"-{currentDate}.");
                                return actualFilePath;
                            }
                            else
                            {
                                // 沒有輪替或其他輪替模式，直接使用原路徑
                                return filePathPattern;
                            }
                        }
                    }
                }

                // 如果無法從配置中讀取，使用預設模式
                var currentDateFallback = DateTime.Now.ToString("yyyy-MM-dd");
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                var fallbackFileName = isDevelopment ? $"northguan-app-{currentDateFallback}.log" : $"app-{currentDateFallback}.log";
                return Path.Combine(_serilogLogPath, fallbackFileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "無法從 Serilog 配置中讀取日誌檔案路徑，使用預設值");
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                var fallbackFileName = isDevelopment ? $"northguan-app-{currentDate}.log" : $"app-{currentDate}.log";
                return Path.Combine(_serilogLogPath, fallbackFileName);
            }
        }

        private (string logPattern, string datePattern) GetSerilogLogPattern()
        {
            try
            {
                // 從 Serilog 配置中讀取 File sink 的路徑模式
                var serilogSection = _configuration.GetSection("Serilog:WriteTo");

                foreach (var writeToSection in serilogSection.GetChildren())
                {
                    var sinkName = writeToSection.GetValue<string>("Name");
                    if (sinkName == "File")
                    {
                        var filePathPattern = writeToSection.GetValue<string>("Args:path");
                        var rollingInterval = writeToSection.GetValue<string>("Args:rollingInterval");

                        if (!string.IsNullOrEmpty(filePathPattern))
                        {
                            var fileName = Path.GetFileName(filePathPattern);

                            // 檢查是否有日期輪替
                            if (rollingInterval == "Day")
                            {
                                // 有日期輪替，檔名會包含日期（格式：yyyyMMdd）
                                if (fileName.Contains("northguan-app-"))
                                {
                                    return ("northguan-app-*.log", @"northguan-app-(\d{8})");
                                }
                                else if (fileName.Contains("app-"))
                                {
                                    return ("app-*.log", @"app-(\d{8})");
                                }
                                else
                                {
                                    // 通用模式：提取檔名前綴
                                    var prefix = fileName.Replace("-.", "").Replace(".log", "");
                                    return ($"{prefix}-*.log", $@"{Regex.Escape(prefix)}-(\d{{8}})");
                                }
                            }
                            else
                            {
                                // 沒有日期輪替，檔名不包含日期，只清理確切的檔名
                                return (fileName, ""); // 空的日期模式表示不按日期清理
                            }
                        }
                    }
                }

                // 如果無法從配置中讀取，使用預設模式
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                return isDevelopment
                    ? ("northguan-app-*.log", @"northguan-app-(\d{4}-\d{2}-\d{2})")
                    : ("app-*.log", @"app-(\d{4}-\d{2}-\d{2})");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "無法從 Serilog 配置中讀取日誌檔案模式，使用預設值");
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                return isDevelopment
                    ? ("northguan-app-*.log", @"northguan-app-(\d{4}-\d{2}-\d{2})")
                    : ("app-*.log", @"app-(\d{4}-\d{2}-\d{2})");
            }
        }

        private async Task CleanupOldLogBackupsAsync()
        {
            try
            {
                _logger.LogInformation("開始清理舊日誌備份檔案");

                var backupPath = Path.Combine(_backupPath, "Logs");
                var retentionDays = _configuration.GetValue<int>("BackupSettings:LogRetentionDays", 30);
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);

                if (!Directory.Exists(backupPath))
                {
                    return;
                }

                var backupFiles = Directory.GetFiles(backupPath, "serilog-backup-*.log");
                var deletedCount = 0;

                foreach (var backupFile in backupFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(backupFile);
                    var match = System.Text.RegularExpressions.Regex.Match(fileName, @"serilog-backup-(\d{14})");

                    if (match.Success)
                    {
                        var dateString = match.Groups[1].Value;

                        if (DateTime.TryParseExact(dateString, "yyyyMMddHHmmss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var fileDate))
                        {
                            if (fileDate < cutoffDate)
                            {
                                File.Delete(backupFile);
                                deletedCount++;
                                Log.Information("刪除舊日誌備份檔案：{BackupFile}", backupFile);
                            }
                        }
                    }
                }

                _logger.LogInformation("清理舊日誌備份檔案完成，刪除 {Count} 個檔案", deletedCount);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理舊日誌備份檔案失敗");
                Log.Error(ex, "清理舊日誌備份檔案失敗");
                throw;
            }
        }

        public async Task<(int cleanedFiles, long freedSpace)> CleanupOldAuditLogsAsync()
        {
            try
            {
                _logger.LogInformation("開始清理舊審計日誌");

                var retentionMonths = _configuration.GetValue<int>("BackupSettings:AuditLogRetentionMonths", 6);
                var currentMonth = DateTime.Now.Year * 12 + DateTime.Now.Month;
                var cutoffMonth = currentMonth - retentionMonths;

                if (!Directory.Exists(_serilogLogPath))
                {
                    return (0, 0);
                }

                // 在 Serilog 日誌目錄中查找審計日誌
                var auditLogFiles = Directory.GetFiles(_serilogLogPath, "northguan-audit-*.log");
                var deletedCount = 0;
                long totalFreedSpace = 0;

                foreach (var logFile in auditLogFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(logFile);

                    // 嘗試從檔名解析日期 (格式: northguan-audit-yyyyMM)
                    var match = System.Text.RegularExpressions.Regex.Match(fileName, @"northguan-audit-(\d{6})");

                    if (match.Success)
                    {
                        var dateString = match.Groups[1].Value;

                        if (dateString.Length == 6 &&
                            int.TryParse(dateString.Substring(0, 4), out var year) &&
                            int.TryParse(dateString.Substring(4, 2), out var month))
                        {
                            var fileMonth = year * 12 + month;

                            if (fileMonth < cutoffMonth)
                            {
                                // 獲取檔案大小
                                var fileInfo = new FileInfo(logFile);
                                if (fileInfo.Exists)
                                {
                                    totalFreedSpace += fileInfo.Length;
                                }

                                File.Delete(logFile);
                                deletedCount++;
                                Log.Information("刪除舊審計日誌：{LogFile}", logFile);
                            }
                        }
                    }
                }

                _logger.LogInformation("清理舊審計日誌完成，刪除 {Count} 個檔案，釋放 {Size} 字節空間", deletedCount, totalFreedSpace);
                Log.Information("清理舊審計日誌完成，刪除 {Count} 個檔案，釋放 {Size} 字節空間", deletedCount, totalFreedSpace);
                await Task.CompletedTask;

                return (deletedCount, totalFreedSpace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理舊審計日誌失敗");
                Log.Error(ex, "清理舊審計日誌失敗");
                throw;
            }
        }

        public async Task GenerateZeroTouchExcelAsync()
        {
            try
            {
                _logger.LogInformation("開始生成零接觸 Excel 檔案");

                var outputPath = Path.Combine(_backupPath, "ZeroTouch");
                Directory.CreateDirectory(outputPath);

                var fileName = $"zero_touch_visitors_{DateTime.Now:yyyyMMdd}.xlsx";
                var filePath = Path.Combine(outputPath, fileName);

                // TODO: 實作 Excel 生成邏輯
                // 這裡需要根據實際的零接觸訪客數據來生成 Excel 檔案
                // 可以使用 EPPlus 或 ClosedXML 等套件

                _logger.LogInformation("零接觸 Excel 檔案生成完成：{FilePath}", filePath);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成零接觸 Excel 檔案失敗");
                throw;
            }
        }

        private async Task CleanupOldDatabaseBackupsAsync()
        {
            try
            {
                _logger.LogInformation("開始清理舊資料庫備份檔案");

                var retentionDays = _configuration.GetValue<int>("BackupSettings:DatabaseBackupRetentionDays", 30);
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);

                if (!Directory.Exists(_backupPath))
                {
                    return;
                }

                // 查找所有資料庫備份檔案（主要是壓縮檔案，也包括可能殘留的原始檔案）
                var gzFiles = Directory.GetFiles(_backupPath, "northguan_*.bak.gz");
                var bakFiles = Directory.GetFiles(_backupPath, "northguan_*.bak");
                var allBackupFiles = gzFiles.Concat(bakFiles).ToArray();
                
                var deletedCount = 0;
                long totalFreedSpace = 0;

                foreach (var backupFile in allBackupFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(backupFile);
                        if (fileInfo.CreationTime < cutoffDate)
                        {
                            totalFreedSpace += fileInfo.Length;
                            File.Delete(backupFile);
                            deletedCount++;
                            _logger.LogInformation("刪除舊資料庫備份檔案：{BackupFile}", backupFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "無法刪除舊資料庫備份檔案：{BackupFile}", backupFile);
                    }
                }

                _logger.LogInformation("清理舊資料庫備份檔案完成，刪除 {Count} 個檔案，釋放 {Size} bytes 空間", 
                    deletedCount, totalFreedSpace);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理舊資料庫備份檔案失敗");
                // 不拋出異常，因為清理失敗不應該影響備份流程
            }
        }

        private async Task<string> CompressBackupFileAsync(string backupFilePath)
        {
            var targetCompressedFilePath = backupFilePath + ".gz";
            
            try
            {
                // 檔案名稱已經在備份開始前確保唯一性，不需要再檢查衝突
                _logger.LogInformation("開始壓縮備份檔案：{BackupFile} -> {CompressedFile}", backupFilePath, targetCompressedFilePath);

                // 等待一小段時間確保檔案不被鎖定
                await Task.Delay(1000);

                // 重試機制：最多嘗試3次
                var maxRetries = 3;
                var retryDelay = 2000; // 2秒

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        using var originalFileStream = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var compressedFileStream = new FileStream(targetCompressedFilePath, FileMode.Create, FileAccess.Write);
                        using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);

                        await originalFileStream.CopyToAsync(compressionStream);
                        
                        // 確保所有資料都寫入
                        await compressionStream.FlushAsync();
                        await compressedFileStream.FlushAsync();
                        
                        break; // 成功，跳出重試迴圈
                    }
                    catch (IOException ioEx) when (attempt < maxRetries)
                    {
                        _logger.LogWarning(ioEx, "壓縮備份檔案失敗，第 {Attempt} 次嘗試，將在 {Delay}ms 後重試", attempt, retryDelay);
                        await Task.Delay(retryDelay);
                        retryDelay *= 2; // 指數退避
                    }
                }

                // 驗證壓縮檔案是否成功創建
                if (!File.Exists(targetCompressedFilePath))
                {
                    throw new InvalidOperationException("壓縮檔案創建失敗");
                }

                var compressedFileInfo = new FileInfo(targetCompressedFilePath);
                if (compressedFileInfo.Length == 0)
                {
                    throw new InvalidOperationException("壓縮檔案大小為零");
                }

                // 嘗試刪除原始備份檔案，如果失敗也不影響整體流程
                try
                {
                    // 等待一小段時間確保檔案句柄已釋放
                    await Task.Delay(500);
                    File.Delete(backupFilePath);
                    _logger.LogInformation("原始備份檔案已刪除：{BackupFile}", backupFilePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "無法刪除原始備份檔案，但壓縮已完成：{BackupFile}", backupFilePath);
                    // 不拋出異常，因為壓縮已經成功
                }

                _logger.LogInformation("備份檔案壓縮完成：{CompressedFile}，大小：{Size} bytes", 
                    targetCompressedFilePath, compressedFileInfo.Length);

                return targetCompressedFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "壓縮備份檔案失敗：{BackupFile}", backupFilePath);
                
                // 清理可能創建的不完整壓縮檔案
                try
                {
                    if (File.Exists(targetCompressedFilePath))
                    {
                        File.Delete(targetCompressedFilePath);
                    }
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "清理不完整壓縮檔案失敗：{CompressedFile}", targetCompressedFilePath);
                }
                
                throw;
            }
        }
    }
}