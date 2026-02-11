namespace northguan_nsa_vue_app.Server.DTOs
{
    /// <summary>
    /// 交通數據同步配置
    /// </summary>
    public class TrafficSyncConfig
    {
        /// <summary>
        /// 數據同步間隔（分鐘）
        /// </summary>
        public int SyncIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// 批次處理大小（一次處理多少個設備）
        /// </summary>
        public int BatchSize { get; set; } = 10;

        /// <summary>
        /// 批次間延遲（秒）
        /// </summary>
        public int BatchDelaySeconds { get; set; } = 5;

        /// <summary>
        /// 請求間延遲（毫秒）- 用於速率控制
        /// </summary>
        public int RequestDelayMs { get; set; } = 500;

        /// <summary>
        /// 是否啟用詳細日誌
        /// </summary>
        public bool EnableVerboseLogging { get; set; } = false;
    }

    /// <summary>
    /// 速率控制狀態
    /// </summary>
    public class RateControlState
    {
        public DateTime LastRequestTime { get; set; } = DateTime.MinValue;
        public int ConsecutiveSuccesses { get; set; } = 0;
        public int ConsecutiveFailures { get; set; } = 0;
        public double CurrentRequestInterval { get; set; } = 2000; // 毫秒
        public bool IsAdaptive { get; set; } = true;
    }
}