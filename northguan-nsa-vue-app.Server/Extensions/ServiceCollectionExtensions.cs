using northguan_nsa_vue_app.Server.Services.ScheduledTasks;
using northguan_nsa_vue_app.Server.Services.Infrastructure;
using northguan_nsa_vue_app.Server.Services.ExternalApi;
using northguan_nsa_vue_app.Server.Services.Export;
using Polly;
using Polly.Extensions.Http;

namespace northguan_nsa_vue_app.Server.Extensions
{
    /// <summary>
    /// 服務註冊擴展方法
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 註冊排程任務相關服務
        /// </summary>
        public static IServiceCollection AddScheduledTaskServices(this IServiceCollection services)
        {
            // 註冊核心排程任務服務
            services.AddScoped<IScheduledTaskService, ScheduledTaskService>();

            // 註冊子服務
            services.AddScoped<DeviceOnlineCheckService>();
            services.AddScoped<DataSyncService>();
            services.AddScoped<BackupService>();
            services.AddScoped<CvpDataSyncService>();

            // 註冊具體數據同步服務
            services.AddScoped<CrowdDataSyncService>();
            services.AddScoped<ParkingDataSyncService>();
            services.AddScoped<TrafficDataSyncService>();

            // 註冊基礎設施服務
            services.AddHttpClient<IDeviceClientService, DeviceClientService>();
            services.AddHttpClient<INotificationService, NotificationService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<IFileManagementService, FileManagementService>();

            // 註冊 HttpContextAccessor
            services.AddHttpContextAccessor();

            // 註冊外部 API 服務
            services.AddScoped<CrowdDataApiService>();
            services.AddHttpClient<ParkingDataApiService>();
            services.AddHttpClient<TransportationUploadService>();

            // 註冊 TDX API 專用 HttpClient 與可配置重試策略
            services.AddHttpClient("TdxApi", (serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var timeoutSeconds = configuration.GetValue<int>("ExternalApi:TDX:TimeoutSeconds", 30);

                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "NSA-Vue-App/1.0");
            })
            .AddPolicyHandler((serviceProvider, request) => GetRetryPolicy(serviceProvider));

            services.AddScoped<TrafficDataApiService>();

            // 註冊背景服務
            services.AddHostedService<ScheduledTaskBackgroundService>();

            return services;
        }

        /// <summary>
        /// 註冊導出服務
        /// </summary>
        public static IServiceCollection AddExportServices(this IServiceCollection services)
        {
            services.AddScoped<IExportService, ExcelExportService>();
            return services;
        }

        /// <summary>
        /// 註冊記憶體快取
        /// </summary>
        public static IServiceCollection AddMemoryCaching(this IServiceCollection services)
        {
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 10000; // 增加快取項目數量限制到 10000
                options.CompactionPercentage = 0.25; // 當達到限制時，清除 25% 的項目
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // 每 5 分鐘掃描過期項目
            });

            return services;
        }

        /// <summary>
        /// 註冊 HTTP 客戶端配置
        /// </summary>
        public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
        {
            services.AddHttpClient("DefaultHttpClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "NorthguanNSA/1.0");
            });

            return services;
        }

        /// <summary>
        /// 獲取 TDX API 重試策略 - 可配置版本，支援指數退避
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("HttpClientRetryPolicy");

            var maxRetries = configuration.GetValue<int>("ExternalApi:TDX:RetryCount", 3);
            var baseDelaySeconds = configuration.GetValue<double>("ExternalApi:TDX:BaseDelaySeconds", 1.0);
            var useExponentialBackoff = configuration.GetValue<bool>("ExternalApi:TDX:UseExponentialBackoff", true);

            return HttpPolicyExtensions
                .HandleTransientHttpError() // 處理 HttpRequestException 和 5XX, 408 狀態碼
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 處理 429 狀態碼
                .WaitAndRetryAsync(
                    retryCount: maxRetries,
                    sleepDurationProvider: retryAttempt =>
                    {
                        if (useExponentialBackoff)
                        {
                            // 指數退避: baseDelay * 2^(retryAttempt-1)
                            return TimeSpan.FromSeconds(baseDelaySeconds * Math.Pow(2, retryAttempt - 1));
                        }
                        else
                        {
                            // 線性延遲: baseDelay * retryAttempt
                            return TimeSpan.FromSeconds(baseDelaySeconds * retryAttempt);
                        }
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        if (outcome.Exception != null)
                        {
                            logger.LogWarning("TDX API 重試 {RetryCount}/{MaxRetries}，延遲 {Delay}s，異常: {Exception}",
                                retryCount, maxRetries, timespan.TotalSeconds, outcome.Exception.Message);
                        }
                        else if (outcome.Result != null)
                        {
                            logger.LogWarning("TDX API 重試 {RetryCount}/{MaxRetries}，延遲 {Delay}s，狀態碼: {StatusCode}",
                                retryCount, maxRetries, timespan.TotalSeconds, outcome.Result.StatusCode);
                        }
                    });
        }

    }
}