using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    /// <summary>
    /// 檔案管理服務接口
    /// </summary>
    public interface IFileManagementService
    {
        /// <summary>
        /// 備份資料庫
        /// </summary>
        Task<(string filePath, long fileSize)> BackupDatabaseAsync();

        /// <summary>
        /// 備份日誌檔案
        /// </summary>
        Task<(int backedUpFiles, long totalSize, string backupLocation)> BackupLogFilesAsync();

        /// <summary>
        /// 清理舊審計日誌
        /// </summary>
        Task<(int cleanedFiles, long freedSpace)> CleanupOldAuditLogsAsync();

        /// <summary>
        /// 生成零接觸 Excel 檔案
        /// </summary>
        Task GenerateZeroTouchExcelAsync();
    }
}