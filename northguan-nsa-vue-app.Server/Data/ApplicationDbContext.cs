using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Extensions;

namespace northguan_nsa_vue_app.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        }

        // Main entities
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserStationPermission> UserStationPermissions { get; set; }
        public DbSet<Station> Stations { get; set; }

        // Device entities
        public DbSet<CrowdDevice> CrowdDevices { get; set; }
        public DbSet<ParkingDevice> ParkingDevices { get; set; }
        public DbSet<TrafficDevice> TrafficDevices { get; set; }
        public DbSet<FenceDevice> FenceDevices { get; set; }
        public DbSet<HighResolutionDevice> HighResolutionDevices { get; set; }
        public DbSet<WaterDevice> WaterDevices { get; set; }

        // Record entities
        public DbSet<ParkingRecord> ParkingRecords { get; set; }
        public DbSet<CrowdRecord> CrowdRecords { get; set; }
        public DbSet<CrowdRecordLatest> CrowdRecordLatests { get; set; }
        public DbSet<TrafficRecord> TrafficRecords { get; set; }
        public DbSet<FenceRecord> FenceRecords { get; set; }
        public DbSet<DeviceStatusLog> DeviceStatusLogs { get; set; }
        public DbSet<ZeroTouchRecord> ZeroTouchRecords { get; set; }
        public DbSet<HighResolutionRecord> HighResolutionRecords { get; set; }
        public DbSet<ECvpTouristRecord> ECvpTouristRecords { get; set; }
        public DbSet<AdmissionRecord> AdmissionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 使用擴展方法自動配置所有時間戳記、精度和字串長度
            modelBuilder.ConfigureTimestamps();
            modelBuilder.ConfigureDecimalPrecision();
            modelBuilder.ConfigureStringLengths();

            // Configure soft delete for Station
            modelBuilder.Entity<Station>()
                .HasQueryFilter(s => s.DeletedAt == null);

            // Configure relationships
            modelBuilder.Entity<CrowdDevice>()
                .HasOne(d => d.Station)
                .WithMany(s => s.CrowdDevices)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParkingDevice>()
                .HasOne(d => d.Station)
                .WithMany(s => s.ParkingDevices)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrafficDevice>()
                .HasOne(d => d.Station)
                .WithMany(s => s.TrafficDevices)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FenceDevice>()
                .HasOne(d => d.Station)
                .WithMany(s => s.FenceDevices)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HighResolutionDevice>()
                .HasOne(d => d.Station)
                .WithMany(s => s.HighResolutionDevices)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WaterDevice>()
                .HasOne(d => d.Station)
                .WithMany(s => s.WaterDevices)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure record-device relationships
            modelBuilder.Entity<CrowdRecord>()
                .HasOne(r => r.CrowdDevice)
                .WithMany()
                .HasForeignKey(r => r.DeviceSerial)
                .HasPrincipalKey(d => d.Serial)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParkingRecord>()
                .HasOne(r => r.ParkingDevice)
                .WithMany()
                .HasForeignKey(r => r.DeviceSerial)
                .HasPrincipalKey(d => d.Serial)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrafficRecord>()
                .HasOne(r => r.TrafficDevice)
                .WithMany()
                .HasForeignKey(r => r.DeviceSerial)
                .HasPrincipalKey(d => d.Serial)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FenceRecord>()
                .HasOne(r => r.FenceDevice)
                .WithMany()
                .HasForeignKey(r => r.DeviceSerial)
                .HasPrincipalKey(d => d.Serial)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for performance (SQL Server optimized)
            modelBuilder.Entity<ParkingRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_ParkingRecords_DeviceSerial");

            modelBuilder.Entity<ParkingRecord>()
                .HasIndex(r => r.Time)
                .HasDatabaseName("IX_ParkingRecords_Time");

            modelBuilder.Entity<CrowdRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_CrowdRecords_DeviceSerial");

            modelBuilder.Entity<CrowdRecord>()
                .HasIndex(r => r.Time)
                .HasDatabaseName("IX_CrowdRecords_Time");

            modelBuilder.Entity<CrowdRecordLatest>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_CrowdRecordLatests_DeviceSerial");

            modelBuilder.Entity<TrafficRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_TrafficRecords_DeviceSerial");

            modelBuilder.Entity<TrafficRecord>()
                .HasIndex(r => r.Time)
                .HasDatabaseName("IX_TrafficRecords_Time");

            modelBuilder.Entity<FenceRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_FenceRecords_DeviceSerial");

            modelBuilder.Entity<FenceRecord>()
                .HasIndex(r => r.Time)
                .HasDatabaseName("IX_FenceRecords_Time");

            modelBuilder.Entity<DeviceStatusLog>()
                .HasIndex(l => new { l.DeviceType, l.DeviceSerial })
                .HasDatabaseName("IX_DeviceStatusLogs_DeviceType_DeviceSerial");

            modelBuilder.Entity<DeviceStatusLog>()
                .HasIndex(l => l.Timestamp)
                .HasDatabaseName("IX_DeviceStatusLogs_Timestamp");

            // Additional indexes for new models
            modelBuilder.Entity<ZeroTouchRecord>()
                .HasIndex(r => r.VisitorId)
                .HasDatabaseName("IX_ZeroTouchRecords_VisitorId");

            modelBuilder.Entity<ZeroTouchRecord>()
                .HasIndex(r => r.VisitTime)
                .HasDatabaseName("IX_ZeroTouchRecords_VisitTime");

            modelBuilder.Entity<HighResolutionRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_HighResolutionRecords_DeviceSerial");

            modelBuilder.Entity<ECvpTouristRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_ECvpTouristRecords_DeviceSerial");

            modelBuilder.Entity<AdmissionRecord>()
                .HasIndex(r => r.DeviceSerial)
                .HasDatabaseName("IX_AdmissionRecords_DeviceSerial");

            // 效能優化：添加複合索引
            ConfigurePerformanceIndexes(modelBuilder);

            // 自動為所有實體的時間戳記屬性設定預設值
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // 跳過 Identity 內建實體 (除了我們的自定義 ApplicationUser 和 ApplicationRole)
                if (entityType.ClrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity") == true &&
                    entityType.ClrType != typeof(ApplicationUser) &&
                    entityType.ClrType != typeof(ApplicationRole))
                {
                    continue;
                }

                // 為 CreatedAt 屬性設定預設值
                var createdAtProperty = entityType.FindProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.ClrType == typeof(DateTime))
                {
                    createdAtProperty.SetDefaultValueSql("GETUTCDATE()");
                }

                // 為 UpdatedAt 屬性設定預設值
                var updatedAtProperty = entityType.FindProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.ClrType == typeof(DateTime))
                {
                    updatedAtProperty.SetDefaultValueSql("GETUTCDATE()");
                }
            }

            // Configure UserStationPermission relationships
            modelBuilder.Entity<UserStationPermission>()
                .HasOne(p => p.User)
                .WithMany(u => u.StationPermissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserStationPermission>()
                .HasOne(p => p.Station)
                .WithMany()
                .HasForeignKey(p => p.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserStationPermission>()
                .HasIndex(p => new { p.UserId, p.StationId })
                .IsUnique()
                .HasDatabaseName("IX_UserStationPermissions_UserId_StationId");

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.Name)
                .HasDatabaseName("IX_Stations_Name");

            // Configure decimal precision for SQL Server
            modelBuilder.Entity<Station>()
                .Property(s => s.Lat)
                .HasPrecision(10, 8);

            modelBuilder.Entity<Station>()
                .Property(s => s.Lng)
                .HasPrecision(11, 8);

            // Apply same precision to all device lat/lng
            foreach (var deviceType in new[] { typeof(CrowdDevice), typeof(ParkingDevice), typeof(TrafficDevice), typeof(FenceDevice), typeof(HighResolutionDevice), typeof(WaterDevice) })
            {
                modelBuilder.Entity(deviceType)
                    .Property("Lat")
                    .HasPrecision(10, 8);

                modelBuilder.Entity(deviceType)
                    .Property("Lng")
                    .HasPrecision(11, 8);
            }

            // Configure decimal precision for SQL Server
            modelBuilder.Entity<TrafficRecord>()
                .Property(r => r.AverageSpeed)
                .HasPrecision(8, 2);

            modelBuilder.Entity<ParkingRecord>()
                .Property(r => r.OccupancyRate)
                .HasPrecision(5, 2);

            // 自動為所有實體的 UpdatedAt 屬性設定為在新增或更新時自動產生值
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // 跳過 Identity 內建實體 (除了我們的自定義實體)
                if (entityType.ClrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity") == true &&
                    entityType.ClrType != typeof(ApplicationUser) &&
                    entityType.ClrType != typeof(ApplicationRole))
                {
                    continue;
                }

                var updatedAtProperty = entityType.FindProperty("UpdatedAt");
                if (updatedAtProperty != null)
                {
                    updatedAtProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate;
                }
            }
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 配置效能優化複合索引
        /// </summary>
        private void ConfigurePerformanceIndexes(ModelBuilder modelBuilder)
        {
            // 1. ParkingRecords 複合索引優化
            modelBuilder.Entity<ParkingRecord>()
                .HasIndex(r => new { r.Time, r.DeviceSerial })
                .HasDatabaseName("IX_ParkingRecords_Time_DeviceSerial_Composite")
                .IsDescending(true, false); // Time 降序，DeviceSerial 升序

            modelBuilder.Entity<ParkingRecord>()
                .HasIndex(r => new { r.DeviceSerial, r.Time })
                .HasDatabaseName("IX_ParkingRecords_DeviceSerial_Time_Composite")
                .IsDescending(false, true); // DeviceSerial 升序，Time 降序

            // 2. CrowdRecords 複合索引優化
            modelBuilder.Entity<CrowdRecord>()
                .HasIndex(r => new { r.Time, r.DeviceSerial })
                .HasDatabaseName("IX_CrowdRecords_Time_DeviceSerial_Composite")
                .IsDescending(true, false);

            modelBuilder.Entity<CrowdRecord>()
                .HasIndex(r => new { r.DeviceSerial, r.Time })
                .HasDatabaseName("IX_CrowdRecords_DeviceSerial_Time_Composite")
                .IsDescending(false, true);

            modelBuilder.Entity<CrowdRecord>()
                .HasIndex(r => new { r.Count, r.Time })
                .HasDatabaseName("IX_CrowdRecords_Count_Time_Composite")
                .IsDescending(false, true);

            // 3. FenceRecords 複合索引優化
            modelBuilder.Entity<FenceRecord>()
                .HasIndex(r => new { r.Time, r.DeviceSerial })
                .HasDatabaseName("IX_FenceRecords_Time_DeviceSerial_Composite")
                .IsDescending(true, false);

            modelBuilder.Entity<FenceRecord>()
                .HasIndex(r => new { r.DeviceSerial, r.Time })
                .HasDatabaseName("IX_FenceRecords_DeviceSerial_Time_Composite")
                .IsDescending(false, true);

            modelBuilder.Entity<FenceRecord>()
                .HasIndex(r => new { r.EventType, r.Time })
                .HasDatabaseName("IX_FenceRecords_EventType_Time_Composite")
                .IsDescending(false, true);

            // 4. TrafficRecords 複合索引優化
            modelBuilder.Entity<TrafficRecord>()
                .HasIndex(r => new { r.Time, r.DeviceSerial })
                .HasDatabaseName("IX_TrafficRecords_Time_DeviceSerial_Composite")
                .IsDescending(true, false);

            modelBuilder.Entity<TrafficRecord>()
                .HasIndex(r => new { r.DeviceSerial, r.Time })
                .HasDatabaseName("IX_TrafficRecords_DeviceSerial_Time_Composite")
                .IsDescending(false, true);

            modelBuilder.Entity<TrafficRecord>()
                .HasIndex(r => new { r.VehicleCount, r.Time })
                .HasDatabaseName("IX_TrafficRecords_VehicleCount_Time_Composite")
                .IsDescending(false, true);

            modelBuilder.Entity<TrafficRecord>()
                .HasIndex(r => new { r.AverageSpeed, r.Time })
                .HasDatabaseName("IX_TrafficRecords_AverageSpeed_Time_Composite")
                .IsDescending(false, true);

            // 5. 設備表複合索引優化
            modelBuilder.Entity<ParkingDevice>()
                .HasIndex(d => new { d.StationId, d.DeletedAt })
                .HasDatabaseName("IX_ParkingDevices_StationId_DeletedAt_Composite");

            modelBuilder.Entity<ParkingDevice>()
                .HasIndex(d => new { d.Name, d.Serial })
                .HasDatabaseName("IX_ParkingDevices_Name_Serial_Composite");

            modelBuilder.Entity<CrowdDevice>()
                .HasIndex(d => new { d.StationId, d.DeletedAt })
                .HasDatabaseName("IX_CrowdDevices_StationId_DeletedAt_Composite");

            modelBuilder.Entity<CrowdDevice>()
                .HasIndex(d => new { d.Name, d.Serial })
                .HasDatabaseName("IX_CrowdDevices_Name_Serial_Composite");

            modelBuilder.Entity<FenceDevice>()
                .HasIndex(d => new { d.StationId, d.DeletedAt })
                .HasDatabaseName("IX_FenceDevices_StationId_DeletedAt_Composite");

            modelBuilder.Entity<FenceDevice>()
                .HasIndex(d => new { d.Name, d.Serial })
                .HasDatabaseName("IX_FenceDevices_Name_Serial_Composite");

            modelBuilder.Entity<TrafficDevice>()
                .HasIndex(d => new { d.StationId, d.DeletedAt })
                .HasDatabaseName("IX_TrafficDevices_StationId_DeletedAt_Composite");

            modelBuilder.Entity<TrafficDevice>()
                .HasIndex(d => new { d.Name, d.Serial })
                .HasDatabaseName("IX_TrafficDevices_Name_Serial_Composite");

            modelBuilder.Entity<HighResolutionDevice>()
                .HasIndex(d => new { d.StationId, d.DeletedAt })
                .HasDatabaseName("IX_HighResolutionDevices_StationId_DeletedAt_Composite");

            modelBuilder.Entity<HighResolutionDevice>()
                .HasIndex(d => new { d.Name, d.Serial })
                .HasDatabaseName("IX_HighResolutionDevices_Name_Serial_Composite");

            modelBuilder.Entity<WaterDevice>()
                .HasIndex(d => new { d.StationId, d.DeletedAt })
                .HasDatabaseName("IX_WaterDevices_StationId_DeletedAt_Composite");

            modelBuilder.Entity<WaterDevice>()
                .HasIndex(d => new { d.Name, d.Serial })
                .HasDatabaseName("IX_WaterDevices_Name_Serial_Composite");

            // 6. Stations 表索引優化
            modelBuilder.Entity<Station>()
                .HasIndex(s => s.DeletedAt)
                .HasDatabaseName("IX_Stations_DeletedAt_Performance");
        }
    }
}