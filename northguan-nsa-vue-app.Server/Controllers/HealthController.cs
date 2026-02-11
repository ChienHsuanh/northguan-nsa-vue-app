using Microsoft.AspNetCore.Mvc;
using northguan_nsa_vue_app.Server.Services;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly DatabaseInitializationService _dbInitService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            DatabaseInitializationService dbInitService,
            IConfiguration configuration,
            ILogger<HealthController> logger)
        {
            _dbInitService = dbInitService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 基本健康檢查端點
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.Now,
                Application = _configuration.GetValue<string>("Application:Name", "北關 NSA Vue 應用系統"),
                Version = _configuration.GetValue<string>("Application:Version", "1.0.0"),
                Environment = _configuration.GetValue<string>("Application:Environment", "Unknown")
            });
        }

        /// <summary>
        /// 詳細健康檢查，包含數據庫狀態
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            try
            {
                var enableHealthCheck = _configuration.GetValue<bool>("Database:EnableHealthCheck", true);
                var dbHealthy = enableHealthCheck ? await _dbInitService.CheckDatabaseHealthAsync() : true;

                var result = new
                {
                    Status = dbHealthy ? "Healthy" : "Unhealthy",
                    Timestamp = DateTime.Now,
                    Application = new
                    {
                        Name = _configuration.GetValue<string>("Application:Name", "北關 NSA Vue 應用系統"),
                        Version = _configuration.GetValue<string>("Application:Version", "1.0.0"),
                        Environment = _configuration.GetValue<string>("Application:Environment", "Unknown")
                    },
                    Database = new
                    {
                        Healthy = dbHealthy,
                        HealthCheckEnabled = enableHealthCheck,
                        AutoMigrate = _configuration.GetValue<bool>("Database:AutoMigrate", true)
                    },
                    Configuration = new
                    {
                        SeedSampleData = _configuration.GetValue<bool>("Database:SeedSampleData", false),
                        JwtExpirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60),
                        MaxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880)
                    }
                };

                return dbHealthy ? Ok(result) : StatusCode(503, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");

                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.Now,
                    Error = "Health check failed",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 數據庫專用健康檢查
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            try
            {
                var enableHealthCheck = _configuration.GetValue<bool>("Database:EnableHealthCheck", true);

                if (!enableHealthCheck)
                {
                    return Ok(new
                    {
                        Status = "Disabled",
                        Message = "Database health check is disabled in configuration",
                        Timestamp = DateTime.Now
                    });
                }

                var isHealthy = await _dbInitService.CheckDatabaseHealthAsync();

                var result = new
                {
                    Status = isHealthy ? "Healthy" : "Unhealthy",
                    Timestamp = DateTime.Now,
                    Database = new
                    {
                        Healthy = isHealthy,
                        AutoMigrate = _configuration.GetValue<bool>("Database:AutoMigrate", true),
                        SeedSampleData = _configuration.GetValue<bool>("Database:SeedSampleData", false)
                    }
                };

                return isHealthy ? Ok(result) : StatusCode(503, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");

                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.Now,
                    Error = "Database health check failed",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 就緒檢查 - 檢查應用程序是否準備好接收流量
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                var enableHealthCheck = _configuration.GetValue<bool>("Database:EnableHealthCheck", true);
                var dbReady = enableHealthCheck ? await _dbInitService.CheckDatabaseHealthAsync() : true;

                // 可以添加其他就緒檢查，如外部服務連接等
                var isReady = dbReady;

                var result = new
                {
                    Status = isReady ? "Ready" : "NotReady",
                    Timestamp = DateTime.Now,
                    Checks = new
                    {
                        Database = dbReady
                        // 可以添加其他檢查項目
                    }
                };

                return isReady ? Ok(result) : StatusCode(503, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");

                return StatusCode(503, new
                {
                    Status = "NotReady",
                    Timestamp = DateTime.Now,
                    Error = "Readiness check failed",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 存活檢查 - 檢查應用程序是否還在運行
        /// </summary>
        [HttpGet("live")]
        public IActionResult GetLiveness()
        {
            // 簡單的存活檢查，只要能響應就表示存活
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.Now,
                Uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()
            });
        }
    }
}