using Microsoft.Extensions.DependencyInjection;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 排程任務背景服務
    /// 負責執行各種定時任務，替代原本 Laravel 的 Console Commands
    /// </summary>
    public class ScheduledTaskBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledTaskBackgroundService> _logger;
        private readonly Dictionary<string, Timer> _timers = new();

        public ScheduledTaskBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ScheduledTaskBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("排程任務背景服務已啟動");

            try
            {
                // 設置各種定時任務
                SetupTimers(stoppingToken);

                // 等待取消信號
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("排程任務背景服務正在停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "排程任務背景服務發生錯誤");
                throw;
            }
            finally
            {
                // 清理所有計時器
                foreach (var timer in _timers.Values)
                {
                    timer?.Dispose();
                }
                _timers.Clear();
            }
        }

        private void SetupTimers(CancellationToken cancellationToken)
        {
            // 每分鐘執行的任務
            SetupMinutelyTasks(cancellationToken);

            // 每小時執行的任務
            SetupHourlyTasks(cancellationToken);

            // 每日執行的任務
            SetupDailyTasks(cancellationToken);

            // 每月執行的任務
            SetupMonthlyTasks(cancellationToken);
        }

        private void SetupMinutelyTasks(CancellationToken cancellationToken)
        {
            // 每分鐘執行的任務
            var minutelyInterval = TimeSpan.FromMinutes(1);
            // 每5分鐘執行的任務 (減少 API 負載)
            var fiveMinuteInterval = TimeSpan.FromMinutes(5);

            // 檢查設備在線狀態
            _timers["CheckDevicesOnline"] = new Timer(
                async _ => await ExecuteTaskSafely("CheckDevicesOnline",
                    async (service) => await service.CheckDevicesOnlineAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 同步人流設備數據
            _timers["SyncCrowdDeviceData"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncCrowdDeviceData",
                    async (service) => await service.SyncCrowdDeviceDataAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 同步停車記錄
            _timers["SyncParkingRecord"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncParkingRecord",
                    async (service) => await service.SyncParkingRecordAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 同步交通 CVP 數據
            _timers["SyncTrafficCvpData"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncTrafficCvpData",
                    async (service) => await service.SyncTrafficCvpDataAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 同步 E2 CVP 數據
            _timers["SyncE2CvpData"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncE2CvpData",
                    async (service) => await service.SyncE2CvpDataAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 同步 G2 CVP 數據
            _timers["SyncG2CvpData"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncG2CvpData",
                    async (service) => await service.SyncG2CvpDataAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 同步零接觸訪客數據
            _timers["SyncZeroTouchVisitor"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncZeroTouchVisitor",
                    async (service) => await service.SyncZeroTouchVisitorAsync()),
                null, TimeSpan.Zero, minutelyInterval);

            // 備份警告日誌
            _timers["BackupWarningLog"] = new Timer(
                async _ => await ExecuteTaskSafely("BackupWarningLog",
                    async (service) => await service.BackupWarningLogAsync()),
                null, TimeSpan.Zero, minutelyInterval);
        }

        private void SetupHourlyTasks(CancellationToken cancellationToken)
        {
            // 每小時執行的任務
            var hourlyInterval = TimeSpan.FromHours(1);

            // 每半小時執行的任務
            var halfHourlyInterval = TimeSpan.FromMinutes(30);

            // 同步交通設備數據 (改為每半小時執行)
            _timers["SyncTrafficDeviceData"] = new Timer(
                async _ => await ExecuteTaskSafely("SyncTrafficDeviceData",
                    async (service) => await service.SyncTrafficDeviceDataAsync()),
                null, TimeSpan.Zero, halfHourlyInterval);
        }

        private void SetupDailyTasks(CancellationToken cancellationToken)
        {
            // 每日執行的任務
            var dailyInterval = TimeSpan.FromDays(1);

            // 備份資料庫
            _timers["BackupDatabase"] = new Timer(
                async _ => await ExecuteTaskSafely("BackupDatabase",
                    async (service) => await service.BackupDatabaseAsync()),
                null, GetNextDailyExecutionTime(), dailyInterval);
        }

        private void SetupMonthlyTasks(CancellationToken cancellationToken)
        {
            // 每月執行的任務
            var monthlyInterval = TimeSpan.FromDays(30);

            // 檢查審計日誌
            _timers["CheckAuditLog"] = new Timer(
                async _ => await ExecuteTaskSafely("CheckAuditLog",
                    async (service) => await service.CheckAuditLogAsync()),
                null, GetNextMonthlyExecutionTime(), monthlyInterval);
        }

        private async Task ExecuteTaskSafely(string taskName, Func<IScheduledTaskService, Task> taskAction)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scheduledTaskService = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();

                _logger.LogDebug("開始執行任務：{TaskName}", taskName);
                var startTime = DateTime.Now;

                await taskAction(scheduledTaskService);

                var executionTime = DateTime.Now - startTime;
                _logger.LogDebug("任務執行完成：{TaskName}，執行時間：{ExecutionTime}", taskName, executionTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "執行任務時發生錯誤：{TaskName}", taskName);
            }
        }

        private static TimeSpan GetNextDailyExecutionTime()
        {
            // 設定每日凌晨 2:00 執行
            var now = DateTime.Now;
            var nextExecution = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);

            if (nextExecution <= now)
            {
                nextExecution = nextExecution.AddDays(1);
            }

            return nextExecution - now;
        }

        private static TimeSpan GetNextMonthlyExecutionTime()
        {
            // 設定每月 1 號凌晨 3:00 執行
            var now = DateTime.Now;
            var nextExecution = new DateTime(now.Year, now.Month, 1, 3, 0, 0);

            if (nextExecution <= now)
            {
                nextExecution = nextExecution.AddMonths(1);
            }

            return nextExecution - now;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止排程任務背景服務");

            // 停止所有計時器
            foreach (var timer in _timers.Values)
            {
                timer?.Dispose();
            }
            _timers.Clear();

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("排程任務背景服務已停止");
        }
    }
}