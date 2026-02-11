using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace northguan_nsa_vue_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdmissionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionCount = table.Column<int>(type: "int", nullable: false),
                    AdmissionType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    EmployeeId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    AvatarFilename = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrowdRecordLatests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalIn = table.Column<int>(type: "int", nullable: false),
                    TotalOut = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrowdRecordLatests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrowdRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalIn = table.Column<int>(type: "int", nullable: false),
                    TotalOut = table.Column<int>(type: "int", nullable: false),
                    In = table.Column<int>(type: "int", nullable: false),
                    Out = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrowdRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatusLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatusLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECvpTouristRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TouristCount = table.Column<int>(type: "int", nullable: false),
                    LocationData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECvpTouristRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FenceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Event = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HighResolutionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImagePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HighResolutionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalSpaces = table.Column<int>(type: "int", nullable: false),
                    OccupiedSpaces = table.Column<int>(type: "int", nullable: false),
                    AvailableSpaces = table.Column<int>(type: "int", nullable: false),
                    OccupancyRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    LineToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    EnableNotify = table.Column<bool>(type: "bit", nullable: false),
                    CvpLocations = table.Column<string>(type: "nvarchar(max)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrafficRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleCount = table.Column<int>(type: "int", nullable: false),
                    AverageSpeed = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    TrafficCondition = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZeroTouchRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    VisitorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VisitTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZeroTouchRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrowdDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Area = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ApiUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    LatestOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrowdDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrowdDevices_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FenceDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObservingTime = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ApiUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    NumberOfParking = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    LatestOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenceDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FenceDevices_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HighResolutionDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    LatestOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HighResolutionDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HighResolutionDevices_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParkingDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    NumberOfParking = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    LatestOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingDevices_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrafficDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ETagNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    SpeedLimit = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    LatestOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficDevices_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStationPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStationPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStationPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStationPermissions_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionRecords_DeviceSerial",
                table: "AdmissionRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrowdDevices_StationId",
                table: "CrowdDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_CrowdRecordLatests_DeviceSerial",
                table: "CrowdRecordLatests",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_CrowdRecords_DeviceSerial",
                table: "CrowdRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_CrowdRecords_StartTime",
                table: "CrowdRecords",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusLogs_DeviceType_DeviceSerial",
                table: "DeviceStatusLogs",
                columns: new[] { "DeviceType", "DeviceSerial" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusLogs_Timestamp",
                table: "DeviceStatusLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ECvpTouristRecords_DeviceSerial",
                table: "ECvpTouristRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_FenceDevices_StationId",
                table: "FenceDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_FenceRecords_DeviceSerial",
                table: "FenceRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_FenceRecords_StartTime",
                table: "FenceRecords",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_HighResolutionDevices_StationId",
                table: "HighResolutionDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_HighResolutionRecords_DeviceSerial",
                table: "HighResolutionRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingDevices_StationId",
                table: "ParkingDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_DeviceSerial",
                table: "ParkingRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingRecords_StartTime",
                table: "ParkingRecords",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_Name",
                table: "Stations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficDevices_StationId",
                table: "TrafficDevices",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficRecords_DeviceSerial",
                table: "TrafficRecords",
                column: "DeviceSerial");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficRecords_StartTime",
                table: "TrafficRecords",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_UserStationPermissions_StationId",
                table: "UserStationPermissions",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStationPermissions_UserId_StationId",
                table: "UserStationPermissions",
                columns: new[] { "UserId", "StationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZeroTouchRecords_VisitorId",
                table: "ZeroTouchRecords",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ZeroTouchRecords_VisitTime",
                table: "ZeroTouchRecords",
                column: "VisitTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdmissionRecords");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CrowdDevices");

            migrationBuilder.DropTable(
                name: "CrowdRecordLatests");

            migrationBuilder.DropTable(
                name: "CrowdRecords");

            migrationBuilder.DropTable(
                name: "DeviceStatusLogs");

            migrationBuilder.DropTable(
                name: "ECvpTouristRecords");

            migrationBuilder.DropTable(
                name: "FenceDevices");

            migrationBuilder.DropTable(
                name: "FenceRecords");

            migrationBuilder.DropTable(
                name: "HighResolutionDevices");

            migrationBuilder.DropTable(
                name: "HighResolutionRecords");

            migrationBuilder.DropTable(
                name: "ParkingDevices");

            migrationBuilder.DropTable(
                name: "ParkingRecords");

            migrationBuilder.DropTable(
                name: "TrafficDevices");

            migrationBuilder.DropTable(
                name: "TrafficRecords");

            migrationBuilder.DropTable(
                name: "UserStationPermissions");

            migrationBuilder.DropTable(
                name: "ZeroTouchRecords");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
