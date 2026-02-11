namespace northguan_nsa_vue_app.Server.Configuration
{
    /// <summary>
    /// 記錄服務配置選項
    /// </summary>
    public class RecordServiceOptions
    {
        public const string SectionName = "RecordService";

        /// <summary>
        /// 默認頁面大小
        /// </summary>
        public int DefaultPageSize { get; set; } = 20;

        /// <summary>
        /// 最大頁面大小
        /// </summary>
        public int MaxPageSize { get; set; } = 1000;

        /// <summary>
        /// 導出最大記錄數
        /// </summary>
        public int MaxExportRecords { get; set; } = 10000;

        /// <summary>
        /// 是否啟用緩存
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// 緩存過期時間（分鐘）
        /// </summary>
        public int CacheExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// 查詢超時時間（秒）
        /// </summary>
        public int QueryTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否啟用查詢日誌
        /// </summary>
        public bool EnableQueryLogging { get; set; } = false;

        /// <summary>
        /// 導出配置
        /// </summary>
        public ExportOptions Export { get; set; } = new();
    }

    /// <summary>
    /// 導出配置選項
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// 支持的導出格式
        /// </summary>
        public List<string> SupportedFormats { get; set; } = new() { "xlsx", "csv" };

        /// <summary>
        /// Excel模板路徑
        /// </summary>
        public string? ExcelTemplatePath { get; set; }

        /// <summary>
        /// 是否包含圖表
        /// </summary>
        public bool IncludeCharts { get; set; } = false;

        /// <summary>
        /// 文件名前綴
        /// </summary>
        public string FileNamePrefix { get; set; } = "Records_";

        /// <summary>
        /// 日期格式
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    }
}