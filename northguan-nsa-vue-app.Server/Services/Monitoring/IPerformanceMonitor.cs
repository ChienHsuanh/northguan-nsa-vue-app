namespace northguan_nsa_vue_app.Server.Services.Monitoring
{
    /// <summary>
    /// 性能監控接口
    /// </summary>
    public interface IPerformanceMonitor
    {
        /// <summary>
        /// 開始監控操作
        /// </summary>
        IDisposable StartOperation(string operationName, Dictionary<string, object>? parameters = null);

        /// <summary>
        /// 記錄查詢性能
        /// </summary>
        Task LogQueryPerformanceAsync(string queryType, TimeSpan duration, int recordCount, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// 記錄導出性能
        /// </summary>
        Task LogExportPerformanceAsync(string exportType, TimeSpan duration, int recordCount, long fileSizeBytes);

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        Task LogErrorAsync(string operation, Exception exception, Dictionary<string, object>? context = null);
    }

    /// <summary>
    /// 性能監控實現
    /// </summary>
    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly ILogger<PerformanceMonitor> _logger;

        public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
        {
            _logger = logger;
        }

        public IDisposable StartOperation(string operationName, Dictionary<string, object>? parameters = null)
        {
            return new OperationTimer(_logger, operationName, parameters);
        }

        public async Task LogQueryPerformanceAsync(string queryType, TimeSpan duration, int recordCount, Dictionary<string, object>? metadata = null)
        {
            await Task.Run(() =>
            {
                _logger.LogInformation("Query Performance: {QueryType} completed in {Duration}ms, returned {RecordCount} records. Metadata: {@Metadata}",
                    queryType, duration.TotalMilliseconds, recordCount, metadata);

                // 如果查詢時間過長，記錄警告
                if (duration.TotalSeconds > 5)
                {
                    _logger.LogWarning("Slow Query Detected: {QueryType} took {Duration}ms", queryType, duration.TotalMilliseconds);
                }
            });
        }

        public async Task LogExportPerformanceAsync(string exportType, TimeSpan duration, int recordCount, long fileSizeBytes)
        {
            await Task.Run(() =>
            {
                _logger.LogInformation("Export Performance: {ExportType} completed in {Duration}ms, exported {RecordCount} records, file size: {FileSizeKB}KB",
                    exportType, duration.TotalMilliseconds, recordCount, fileSizeBytes / 1024);
            });
        }

        public async Task LogErrorAsync(string operation, Exception exception, Dictionary<string, object>? context = null)
        {
            await Task.Run(() =>
            {
                _logger.LogError(exception, "Error in operation: {Operation}. Context: {@Context}", operation, context);
            });
        }

        private class OperationTimer : IDisposable
        {
            private readonly ILogger _logger;
            private readonly string _operationName;
            private readonly Dictionary<string, object>? _parameters;
            private readonly DateTime _startTime;
            private bool _disposed;

            public OperationTimer(ILogger logger, string operationName, Dictionary<string, object>? parameters)
            {
                _logger = logger;
                _operationName = operationName;
                _parameters = parameters;
                _startTime = DateTime.Now;

                _logger.LogDebug("Starting operation: {OperationName} with parameters: {@Parameters}",
                    _operationName, _parameters);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    var duration = DateTime.Now - _startTime;
                    _logger.LogDebug("Completed operation: {OperationName} in {Duration}ms",
                        _operationName, duration.TotalMilliseconds);

                    _disposed = true;
                }
            }
        }
    }
}