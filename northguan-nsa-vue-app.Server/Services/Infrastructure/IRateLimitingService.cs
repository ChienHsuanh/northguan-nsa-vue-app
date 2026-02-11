using northguan_nsa_vue_app.Server.Middleware;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    /// <summary>
    /// 速率控制服務介面
    /// </summary>
    public interface IRateLimitingService
    {
        /// <summary>
        /// 應用速率控制
        /// </summary>
        Task ApplyRateLimitAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 記錄成功請求
        /// </summary>
        Task RecordSuccessAsync(string key);

        /// <summary>
        /// 記錄失敗請求
        /// </summary>
        Task RecordFailureAsync(string key, Exception? exception = null);

        /// <summary>
        /// 檢查熔斷器狀態
        /// </summary>
        bool IsCircuitBreakerOpen(string key);

        /// <summary>
        /// 獲取速率控制統計
        /// </summary>
        RateLimitStats GetStats(string key);

        /// <summary>
        /// 重置速率控制狀態
        /// </summary>
        Task ResetAsync(string key);
    }

    /// <summary>
    /// 速率控制統計
    /// </summary>
    public class RateLimitStats
    {
        public string Key { get; set; } = string.Empty;
        public long TotalRequests { get; set; }
        public long SuccessfulRequests { get; set; }
        public long FailedRequests { get; set; }
        public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;
        public double CurrentIntervalMs { get; set; }
        public int ConsecutiveSuccesses { get; set; }
        public int ConsecutiveFailures { get; set; }
        public DateTime LastRequestTime { get; set; }
        public bool IsCircuitBreakerOpen { get; set; }
        public DateTime? CircuitBreakerOpenTime { get; set; }
    }
}