using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.Extensions;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public class DatabaseInitializationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<DatabaseInitializationService> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseInitializationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<DatabaseInitializationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting database initialization...");

                // Ensure database is created and migrations are applied
                await EnsureDatabaseAsync();

                // Create default roles
                await CreateDefaultRolesAsync();

                // Create default admin user
                await CreateDefaultAdminAsync();

                // Seed sample data if configured
                if (_configuration.GetValue<bool>("Database:SeedSampleData", false))
                {
                    await SeedSampleDataAsync();
                }

                _logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private async Task EnsureDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Checking database connection...");

                // Check if database can be connected
                if (await _context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Database connection successful.");
                }
                else
                {
                    _logger.LogWarning("Cannot connect to database, attempting to create...");
                }

                // Check AutoMigrate configuration
                var autoMigrate = _configuration.GetValue<bool>("Database:AutoMigrate", true);

                if (autoMigrate)
                {
                    // Apply pending migrations
                    var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        _logger.LogInformation("AutoMigrate enabled. Applying {Count} pending migrations: {Migrations}",
                            pendingMigrations.Count(), string.Join(", ", pendingMigrations));

                        await _context.Database.MigrateAsync();
                        _logger.LogInformation("Database migrations applied successfully.");
                    }
                    else
                    {
                        _logger.LogInformation("Database is up to date, no migrations needed.");
                    }
                }
                else
                {
                    _logger.LogInformation("AutoMigrate disabled. Skipping automatic database migrations.");

                    // Still check for pending migrations and warn
                    var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        _logger.LogWarning("There are {Count} pending migrations that need to be applied manually: {Migrations}",
                            pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure database is ready.");
                throw;
            }
        }

        private async Task CreateDefaultRolesAsync()
        {
            var defaultRoles = new[] { "Admin", "User", "Manager", "Operator" };

            foreach (var roleName in defaultRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole { Name = roleName };
                    var result = await _roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private async Task CreateDefaultAdminAsync()
        {
            var adminEmail = _configuration.GetValue<string>("DefaultAdmin:Email") ?? "admin@yourproject.com";
            var adminUsername = _configuration.GetValue<string>("DefaultAdmin:Username") ?? "admin";
            var adminPassword = _configuration.GetValue<string>("DefaultAdmin:Password") ?? "Admin123!";
            var adminName = _configuration.GetValue<string>("DefaultAdmin:Name") ?? "系統管理員";

            _logger.LogInformation("Creating default admin user with email: {Email}", adminEmail);

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    Name = adminName,
                    EmailConfirmed = true,
                    ReadOnly = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);

                    // Add admin to all existing stations
                    await GrantAdminStationAccessAsync(adminUser.Id);

                    // Log admin credentials (only in development)
                    if (_configuration.GetValue<bool>("Logging:LogAdminCredentials", false))
                    {
                        _logger.LogInformation("Default admin credentials - Email: {Email}, Password: {Password}",
                            adminEmail, adminPassword);
                    }
                }
                else
                {
                    _logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                _logger.LogInformation("Admin user already exists: {Email}", adminEmail);

                // Ensure admin has Admin role
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Added Admin role to existing admin user.");
                }

                // Ensure admin has access to all stations
                await GrantAdminStationAccessAsync(adminUser.Id);
            }
        }

        private async Task GrantAdminStationAccessAsync(string adminUserId)
        {
            try
            {
                var allStations = await _context.Stations.Select(s => s.Id).ToListAsync();
                var existingPermissions = await _context.UserStationPermissions
                    .Where(p => p.UserId == adminUserId)
                    .Select(p => p.StationId)
                    .ToListAsync();

                var newStations = allStations.Except(existingPermissions).ToList();

                if (newStations.Any())
                {
                    foreach (var stationId in newStations)
                    {
                        _context.UserStationPermissions.Add(new UserStationPermission
                        {
                            UserId = adminUserId,
                            StationId = stationId
                        });
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Granted admin access to {Count} new stations", newStations.Count);
                }
                else
                {
                    _logger.LogInformation("Admin already has access to all existing stations");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to grant admin station access");
                throw;
            }
        }

        #region Sample data generation
        public async Task SeedSampleDataAsync()
        {
            // Only seed if no data exists
            if (await _context.Stations.AnyAsync())
            {
                return;
            }

            _logger.LogInformation("Seeding sample data...");

            // Create sample stations
            var stations = new[]
            {
                new Station { Name = "台北車站", Lat = 25.04776000m, Lng = 121.51697000m, EnableNotify = true, CvpLocations = "台北市中正區" },
                new Station { Name = "信義商圈", Lat = 25.03369000m, Lng = 121.56457000m, EnableNotify = true, CvpLocations = "台北市信義區" },
                new Station { Name = "西門町", Lat = 25.04217000m, Lng = 121.50684000m, EnableNotify = false, CvpLocations = "台北市萬華區" }
            };

            _context.Stations.AddRange(stations);
            await _context.SaveChangesAsync();

            // Create sample devices for each station
            foreach (var station in stations)
            {
                // Crowd devices
                _context.CrowdDevices.Add(new CrowdDevice
                {
                    Name = $"{station.Name}-人流監測1",
                    StationId = station.Id,
                    Serial = $"CROWD{station.Id:D3}",
                    Lat = station.Lat,
                    Lng = station.Lng,
                    Status = "online",
                    Area = 1000,
                    VideoUrl = $"http://example.com/video{station.Id}",
                    ApiUrl = $"http://example.com/api{station.Id}"
                });

                // Parking devices
                _context.ParkingDevices.Add(new ParkingDevice
                {
                    Name = $"{station.Name}停車場",
                    StationId = station.Id,
                    Serial = $"PARK{station.Id:D3}",
                    Lat = station.Lat,
                    Lng = station.Lng,
                    Status = "online",
                    NumberOfParking = 200,
                    ApiUrl = $"http://example.com/parking{station.Id}"
                });

                // Traffic devices
                _context.TrafficDevices.Add(new TrafficDevice
                {
                    Name = $"{station.Name}車流監測",
                    StationId = station.Id,
                    Serial = $"TRAFFIC{station.Id:D3}",
                    Lat = station.Lat,
                    Lng = station.Lng,
                    Status = "online",
                    City = "台北市",
                    ETagNumber = $"ETAG{station.Id:D3}",
                    SpeedLimit = 50
                });

                // Fence devices
                _context.FenceDevices.Add(new FenceDevice
                {
                    Name = $"{station.Name}電子圍籬",
                    StationId = station.Id,
                    Serial = $"FENCE{station.Id:D3}",
                    Lat = station.Lat,
                    Lng = station.Lng,
                    Status = "online",
                    ObservingTimeStart = "08:00",
                    ObservingTimeEnd = "18:00"
                });

                // High resolution devices
                _context.HighResolutionDevices.Add(new HighResolutionDevice
                {
                    Name = $"{station.Name}4K攝影機",
                    StationId = station.Id,
                    Serial = $"HR{station.Id:D3}",
                    Lat = station.Lat,
                    Lng = station.Lng,
                    Status = "online",
                    VideoUrl = $"http://example.com/4k-video{station.Id}"
                });
            }

            await _context.SaveChangesAsync();

            // Add sample records with more realistic data
            SeedSampleRecords();

            await _context.SaveChangesAsync();

            // Grant admin access to all stations
            var adminUser = await _userManager.FindByEmailAsync("admin@yourproject.com");
            if (adminUser != null)
            {
                var allStationIds = await _context.Stations.Select(s => s.Id).ToListAsync();
                foreach (var stationId in allStationIds)
                {
                    if (!await _context.UserStationPermissions.AnyAsync(p => p.UserId == adminUser.Id && p.StationId == stationId))
                    {
                        _context.UserStationPermissions.Add(new UserStationPermission
                        {
                            UserId = adminUser.Id,
                            StationId = stationId
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Sample data seeded successfully.");
        }

        private void SeedSampleRecords()
        {
            var currentTime = DateTime.Now;
            var random = new Random();

            _logger.LogInformation("Seeding sample records...");

            // Generate historical data for the past 7 days
            for (int day = 7; day >= 0; day--)
            {
                var dayTime = currentTime.AddDays(-day);

                // Generate records for each hour of the day
                for (int hour = 0; hour < 24; hour++)
                {
                    var hourTime = dayTime.AddHours(hour);

                    // Sample parking records with realistic patterns
                    for (int i = 1; i <= 3; i++)
                    {
                        var baseParked = GetRealisticParkingCount(hour, i);
                        var variance = random.Next(-20, 21); // ±20 variance
                        var parkedNum = Math.Max(0, Math.Min(200, baseParked + variance));

                        _context.ParkingRecords.Add(new ParkingRecord
                        {
                            DeviceSerial = $"PARK{i:D3}",
                            Time = hourTime,
                            TotalSpaces = 200,
                            OccupiedSpaces = parkedNum,
                            AvailableSpaces = 200 - parkedNum,
                            OccupancyRate = (decimal)(parkedNum / 200.0),
                            CreatedAt = hourTime,
                            UpdatedAt = hourTime
                        });
                    }

                    // Sample crowd records with realistic patterns
                    for (int i = 1; i <= 3; i++)
                    {
                        var baseCrowd = GetRealisticCrowdCount(hour, i);
                        var variance = random.Next(-100, 101); // ±100 variance
                        var peopleCount = Math.Max(0, baseCrowd + variance);
                        var totalIn = random.Next(peopleCount, peopleCount + 500);
                        var totalOut = totalIn - peopleCount;

                        _context.CrowdRecords.Add(new CrowdRecord
                        {
                            DeviceSerial = $"CROWD{i:D3}",
                            Time = hourTime,
                            TotalIn = totalIn,
                            TotalOut = totalOut,
                            In = random.Next(0, 50),
                            Out = random.Next(0, 50),
                            Count = peopleCount,
                            CreatedAt = hourTime,
                            UpdatedAt = hourTime
                        });
                    }

                    // Sample traffic records
                    for (int i = 1; i <= 3; i++)
                    {
                        var baseVehicles = GetRealisticTrafficCount(hour);
                        var vehicleVariance = random.Next(-10, 11);
                        var vehicleCount = Math.Max(0, baseVehicles + vehicleVariance);

                        var baseSpeed = GetRealisticSpeed(hour);
                        var speedVariance = random.Next(-10, 11);
                        var averageSpeed = Math.Max(10, Math.Min(80, baseSpeed + speedVariance));

                        _context.TrafficRecords.Add(new TrafficRecord
                        {
                            DeviceSerial = $"TRAFFIC{i:D3}",
                            VehicleCount = vehicleCount,
                            AverageSpeed = averageSpeed,
                            TravelTime = averageSpeed > 30 ? random.Next(10, 30) : random.Next(30, 60), // 添加 TravelTime 欄位
                            TrafficCondition = averageSpeed > 40 ? "smooth" : averageSpeed > 25 ? "normal" : "congested",
                            Time = hourTime,
                            CreatedAt = hourTime,
                            UpdatedAt = hourTime
                        });
                    }

                    // Sample fence records (less frequent)
                    if (random.Next(1, 101) <= 10) // 10% chance per hour
                    {
                        var stationIndex = random.Next(1, 4);
                        var events = new[] { "入侵檢測", "區域警報", "設備異常", "正常巡檢" };
                        var eventType = events[random.Next(events.Length)];

                        _context.FenceRecords.Add(new FenceRecord
                        {
                            DeviceSerial = $"FENCE{stationIndex:D3}",
                            EventType = random.Next(1, 3) == 1 ? FenceEventType.Enter : FenceEventType.Exit,
                            Time = hourTime
                        });
                    }
                }
            }

            _logger.LogInformation("Sample records generated for the past 7 days");
        }

        private int GetRealisticParkingCount(int hour, int stationMultiplier)
        {
            // Simulate realistic parking patterns throughout the day
            var baseCounts = new int[] { 50, 40, 30, 25, 30, 50, 80, 120, 150, 170, 180, 185, 190, 185, 180, 175, 170, 160, 140, 120, 100, 80, 70, 60 };
            return baseCounts[hour] + (stationMultiplier * 10);
        }

        private int GetRealisticCrowdCount(int hour, int stationMultiplier)
        {
            // Simulate realistic crowd patterns throughout the day
            var baseCounts = new int[] { 100, 50, 30, 20, 25, 80, 200, 400, 600, 800, 900, 950, 1000, 950, 900, 850, 800, 700, 600, 500, 400, 300, 200, 150 };
            return baseCounts[hour] + (stationMultiplier * 50);
        }

        private int GetRealisticTrafficCount(int hour)
        {
            // Simulate realistic traffic patterns throughout the day
            var baseCounts = new int[] { 10, 5, 3, 2, 3, 8, 25, 45, 60, 55, 50, 52, 55, 53, 50, 55, 60, 65, 55, 45, 35, 25, 20, 15 };
            return baseCounts[hour];
        }

        private int GetRealisticSpeed(int hour)
        {
            // Simulate realistic speed patterns (slower during rush hours)
            var baseSpeeds = new int[] { 45, 50, 50, 50, 48, 40, 25, 20, 25, 35, 40, 38, 35, 38, 40, 35, 25, 20, 25, 35, 40, 45, 45, 45 };
            return baseSpeeds[hour];
        }

        /// <summary>
        /// 清理所有示例數據（謹慎使用）
        /// </summary>
        public async Task CleanSampleDataAsync()
        {
            _logger.LogWarning("Starting to clean sample data...");

            try
            {
                // Remove all records
                _context.ParkingRecords.RemoveRange(_context.ParkingRecords);
                _context.CrowdRecords.RemoveRange(_context.CrowdRecords);
                _context.TrafficRecords.RemoveRange(_context.TrafficRecords);
                _context.FenceRecords.RemoveRange(_context.FenceRecords);

                // Remove all devices
                _context.ParkingDevices.RemoveRange(_context.ParkingDevices);
                _context.CrowdDevices.RemoveRange(_context.CrowdDevices);
                _context.TrafficDevices.RemoveRange(_context.TrafficDevices);
                _context.FenceDevices.RemoveRange(_context.FenceDevices);
                _context.HighResolutionDevices.RemoveRange(_context.HighResolutionDevices);

                // Remove all stations except those with real users
                var stationsWithUsers = await _context.UserStationPermissions
                    .Select(p => p.StationId)
                    .Distinct()
                    .ToListAsync();

                var stationsToRemove = await _context.Stations
                    .Where(s => !stationsWithUsers.Contains(s.Id))
                    .ToListAsync();

                _context.Stations.RemoveRange(stationsToRemove);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Sample data cleaned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning sample data");
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 檢查數據庫健康狀態
        /// </summary>
        public async Task<bool> CheckDatabaseHealthAsync()
        {
            var enableHealthCheck = _configuration.GetValue<bool>("Database:EnableHealthCheck", true);

            if (!enableHealthCheck)
            {
                _logger.LogInformation("Database health check is disabled in configuration");
                return true;
            }

            try
            {
                _logger.LogInformation("Starting database health check...");

                // Check database connection
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database");
                    return false;
                }

                // Check if required tables exist and get counts
                var stationCount = await _context.Stations.CountAsync();
                var userCount = await _context.Users.CountAsync();
                var roleCount = await _context.Roles.CountAsync();

                // Check device tables
                var parkingDeviceCount = await _context.ParkingDevices.CountAsync();
                var crowdDeviceCount = await _context.CrowdDevices.CountAsync();
                var trafficDeviceCount = await _context.TrafficDevices.CountAsync();
                var fenceDeviceCount = await _context.FenceDevices.CountAsync();
                var hrDeviceCount = await _context.HighResolutionDevices.CountAsync();

                // Check record tables
                var parkingRecordCount = await _context.ParkingRecords.CountAsync();
                var crowdRecordCount = await _context.CrowdRecords.CountAsync();
                var trafficRecordCount = await _context.TrafficRecords.CountAsync();
                var fenceRecordCount = await _context.FenceRecords.CountAsync();

                // Check for pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var hasPendingMigrations = pendingMigrations.Any();

                _logger.LogInformation("Database health check results:");
                _logger.LogInformation("  Core Tables - Stations: {StationCount}, Users: {UserCount}, Roles: {RoleCount}",
                    stationCount, userCount, roleCount);
                _logger.LogInformation("  Device Tables - Parking: {ParkingDevices}, Crowd: {CrowdDevices}, Traffic: {TrafficDevices}, Fence: {FenceDevices}, HR: {HRDevices}",
                    parkingDeviceCount, crowdDeviceCount, trafficDeviceCount, fenceDeviceCount, hrDeviceCount);
                _logger.LogInformation("  Record Tables - Parking: {ParkingRecords}, Crowd: {CrowdRecords}, Traffic: {TrafficRecords}, Fence: {FenceRecords}",
                    parkingRecordCount, crowdRecordCount, trafficRecordCount, fenceRecordCount);

                if (hasPendingMigrations)
                {
                    _logger.LogWarning("Pending migrations detected: {Migrations}", string.Join(", ", pendingMigrations));
                }
                else
                {
                    _logger.LogInformation("Database schema is up to date");
                }

                // Health check passes if we can connect and have basic data
                var isHealthy = canConnect && roleCount > 0;

                if (isHealthy)
                {
                    _logger.LogInformation("Database health check passed");
                }
                else
                {
                    _logger.LogError("Database health check failed - missing required data");
                }

                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed with exception");
                return false;
            }
        }
    }
}