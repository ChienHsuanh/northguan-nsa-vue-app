using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using Microsoft.AspNetCore.Identity;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.Extensions;
using Microsoft.AspNetCore.Authentication;
using NSwag;
using NSwag.Generation.Processors.Security;
using Microsoft.AspNetCore.Mvc;
using northguan_nsa_vue_app.Server.Middleware;
using northguan_nsa_vue_app.Server.NSwag;
using northguan_nsa_vue_app.Server.Authentication;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using FluentValidation;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Explicitly set WebRootPath to ensure it's not null
    builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

    // Add Serilog to the logging pipeline
    // builder.Host.UseSerilog();
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)  //從設定檔中讀取
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.With(new LogEnricher())
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e =>
            e.Properties.ContainsKey("ControllerName") &&
            e.Properties["ControllerName"].ToString().Contains("Controller"))
            .WriteTo.File(builder.Configuration["Serilog:WriteTo:2:Args:Path"] ?? "logs/controller-.log",
                rollingInterval: Enum.TryParse<RollingInterval>(builder.Configuration["Serilog:WriteTo:2:Args:rollingInterval"], out var ri1) ? ri1 : RollingInterval.Day,
                retainedFileCountLimit: int.TryParse(builder.Configuration["Serilog:WriteTo:2:Args:retainedFileCountLimit"], out var rc1) ? rc1 : 7,
                outputTemplate: builder.Configuration["Serilog:WriteTo:2:Args:outputTemplate"] ?? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
        .WriteTo.Logger(lc=>lc.Filter.ByExcluding(e=>
            e.Properties.ContainsKey("SourceContext") &&
            e.Properties["SourceContext"].ToString().Contains("Controller"))
            .WriteTo.File(builder.Configuration["Serilog:WriteTo:3:Args:Path"] ?? "logs/app-.log",
                rollingInterval: Enum.TryParse<RollingInterval>(builder.Configuration["Serilog:WriteTo:3:Args:rollingInterval"], out var ri2) ? ri2 : RollingInterval.Day,
                retainedFileCountLimit: int.TryParse(builder.Configuration["Serilog:WriteTo:3:Args:retainedFileCountLimit"], out var rc2) ? rc2 : 7))
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e =>
            e.Properties.ContainsKey("SourceContext") &&
            e.Properties["SourceContext"].ToString().Contains("Controller"))
            .WriteTo.File(new CompactJsonFormatter(), builder.Configuration["Serilog:WriteTo:5:Args:Path"] ?? "logs/controller-compact-.log",
                rollingInterval: Enum.TryParse<RollingInterval>(builder.Configuration["Serilog:WriteTo:5:Args:rollingInterval"], out var ri3) ? ri3 : RollingInterval.Day,
                retainedFileCountLimit: int.TryParse(builder.Configuration["Serilog:WriteTo:5:Args:retainedFileCountLimit"], out var rc3) ? rc3 : 7))
    );

    // Add services to the container.
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    ));

    // Configure CORS
    var allOrigins = "allowAll";
    builder.Services.AddCors(option =>
        option.AddPolicy(name: allOrigins,
            policy =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    policy.WithOrigins("http://localhost:13343", "https://localhost:13343")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    // Production CORS - configure based on your domain
                    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost", "https://localhost" };
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            }
        )
    );

    builder.Services.AddControllers(options =>
    {
        // 添加統一的驗證過濾器 (處理 ModelState 和 FluentValidation)
        options.Filters.Add<northguan_nsa_vue_app.Server.Filters.ValidationFilter>();
    });

    // 配置 API 行為選項，禁用預設的模型驗證回應
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        // 禁用預設的模型驗證錯誤回應，讓我們的 ModelValidationFilter 處理
        options.SuppressModelStateInvalidFilter = true;
    });

    // 註冊統一驗證過濾器
    builder.Services.AddScoped<northguan_nsa_vue_app.Server.Filters.ValidationFilter>();

    builder.Services.AddAuthorization();

    // Add HttpContextAccessor for accessing HTTP context in services
    builder.Services.AddHttpContextAccessor();

    // Register services (you'll need to implement these)
    builder.Services.AddScoped<IPasetoService, PasetoService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddScoped<IStationService, StationService>();
    builder.Services.AddScoped<IDeviceService, DeviceService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IMapService, MapService>();

    // Overview services - 使用拆分後的服務
    builder.Services.AddScoped<IParkingOverviewService, ParkingOverviewService>();
    builder.Services.AddScoped<ICrowdOverviewService, CrowdOverviewService>();
    builder.Services.AddScoped<IFenceOverviewService, FenceOverviewService>();
    builder.Services.AddScoped<ITrafficOverviewService, TrafficOverviewService>();
    builder.Services.AddScoped<IHighResolutionOverviewService, HighResolutionOverviewService>();

    // Add record service configuration
    builder.Services.Configure<northguan_nsa_vue_app.Server.Configuration.RecordServiceOptions>(
        builder.Configuration.GetSection(northguan_nsa_vue_app.Server.Configuration.RecordServiceOptions.SectionName));

    // Add record services (refactored architecture - now enabled!)
    // builder.Services.AddRecordServices();
    // builder.Services.AddValidationServices();
    // builder.Services.AddCachingServices();

    // Add performance monitoring
    builder.Services.AddScoped<northguan_nsa_vue_app.Server.Services.Monitoring.IPerformanceMonitor,
        northguan_nsa_vue_app.Server.Services.Monitoring.PerformanceMonitor>();

    // Add FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<northguan_nsa_vue_app.Server.Validators.FenceRecordQueryParametersValidator>();

    // Record services
    builder.Services.AddScoped<IFenceRecordService, FenceRecordService>();
    builder.Services.AddScoped<ICrowdRecordService, CrowdRecordService>();
    builder.Services.AddScoped<IParkingRecordService, ParkingRecordService>();
    builder.Services.AddScoped<ITrafficRecordService, TrafficRecordService>();

    // Other services
    builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
    builder.Services.AddScoped<IThirdPartyService, ThirdPartyService>();

    // Database initialization service
    builder.Services.AddScoped<DatabaseInitializationService>();

    // Add scheduled task services
    builder.Services.AddScheduledTaskServices();

    // Add export services
    builder.Services.AddExportServices();

    // Add memory caching
    builder.Services.AddMemoryCaching();

    // Add HTTP client configuration
    builder.Services.AddHttpClientConfiguration();

    // Add health checks
    builder.Services.AddHealthChecks();

    // Configure Identity
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Configure Authentication (will use PASETO)
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "PASETO";
        options.DefaultChallengeScheme = "PASETO";
        options.DefaultScheme = "PASETO";
    })
    .AddScheme<AuthenticationSchemeOptions, PasetoAuthenticationHandler>("PASETO", options => { });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Northguan NSA Vue App API";
        config.Version = "v1";

        // 設定 JWT Bearer 驗證
        config.AddSecurity("Bearer", new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.Http,
            Name = "Authorization",
            Scheme = "Bearer",
            Description = "Copy this into the value field: Bearer {token}"
        });

        // 將驗證應用於所有 API 操作
        config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));

        // 添加簡化的錯誤回應處理器
        config.OperationProcessors.Add(new SimpleErrorResponseProcessor());
    });

    var app = builder.Build();

    // 使用自訂的全域錯誤處理中介軟體
    app.UseGlobalExceptionHandling();

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        // Customize log level
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode > 499)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode > 399)
                return LogEventLevel.Warning;

            if (elapsed > 1000)
                return LogEventLevel.Warning;

            return LogEventLevel.Information;
        };

        // Enrich log context with additional properties
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            }
        };
    });

    // Initialize database and seed data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Get the database initialization service from DI container
            var dbInitializer = services.GetRequiredService<DatabaseInitializationService>();

            // Initialize database (respects AutoMigrate configuration)
            await dbInitializer.InitializeAsync();

            // Perform health check if enabled
            var configuration = services.GetRequiredService<IConfiguration>();
            var enableHealthCheck = configuration.GetValue<bool>("Database:EnableHealthCheck", true);

            if (enableHealthCheck)
            {
                var isHealthy = await dbInitializer.CheckDatabaseHealthAsync();
                if (!isHealthy)
                {
                    logger.LogWarning("Database health check failed during startup");
                }
            }

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");

            // In production, you might want to fail fast if database initialization fails
            if (!app.Environment.IsDevelopment())
            {
                throw;
            }
        }
    }

    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Configure static file serving for uploads
    var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");

    // Ensure uploads directory exists
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }

    // Serve files from uploads directory
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });

    // Configure the HTTP request pipeline.
    // if (app.Environment.IsDevelopment())
    // {
        app.UseOpenApi();
        app.UseSwaggerUi();
    // }

    app.UseCors(allOrigins);

    // Configure HTTPS settings
    var httpsConfig = builder.Configuration.GetSection("Https");
    var httpsEnabled = httpsConfig.GetValue<bool>("Enabled", false);
    var httpsRedirect = httpsConfig.GetValue<bool>("RedirectFromHttp", false);

    // Conditional HTTPS redirection
    if (httpsEnabled && httpsRedirect)
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Map health check endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Only basic liveness check
    });

    // Configure SPA fallback for Vue.js routing
    // For any non-API routes, serve the index.html to let Vue Router handle client-side routing
    app.MapFallback(async context =>
    {
        // Skip API routes
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 404;
            return;
        }

        // Serve index.html for SPA routes
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
    });

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
