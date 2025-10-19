using BookingService.Application;
using BookingService.Infrastructure;
using EventBus.Extensions;
using Identity.Shared;
using MassTransit;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Auditing;
using SharedKernel.Caching;
using SharedKernel.Middleware;
using SharedKernel.RateLimiting;
using SharedKernel.SignalR;
using SharedKernel.Versioning;
using Tenancy;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.WithProperty("Service", "BookingService")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";

var jwtSettings = new JwtSettings
{
    SecretKey = builder.Configuration["Jwt:SecretKey"]!,
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "TravelPortal",
    Audience = builder.Configuration["Jwt:Audience"] ?? "TravelPortal"
};

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Booking Service API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SharedKernel.Behaviors.ICorrelationIdProvider, SharedKernel.Behaviors.CorrelationIdProvider>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Distributed Cache (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "TravelPortal:";
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddMultiTenancy();
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddTenantRateLimiting();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

// SignalR for real-time notifications
builder.Services.AddSignalRNotifications(redisConnection);
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();

// Event Bus with SignalR event handlers
builder.Services.AddEventBus(rabbitMqHost, "guest", "guest");

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database", tags: new[] { "db" })
    .AddRedis(redisConnection, name: "redis", tags: new[] { "cache" });

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                // Allow localhost with any port
                if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:"))
                    return true;
                
                // Allow *.localhost subdomains for tenant isolation
                if (origin.Contains(".localhost:"))
                    return true;
                
                return false;
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

BookingService.Infrastructure.DependencyInjection.InitializeDatabase(connectionString);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseCors();  // Enable CORS
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();  // Global exception handling

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

