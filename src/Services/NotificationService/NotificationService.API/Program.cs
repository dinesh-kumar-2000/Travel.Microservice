using NotificationService.Application;
using NotificationService.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Middleware;
using SharedKernel.Caching;
using SharedKernel.Versioning;
using SharedKernel.SignalR;
using EventBus.Extensions;
using Identity.Shared;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.WithProperty("Service", "NotificationService")
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
    c.SwaggerDoc("v1", new() { Title = "Notification Service API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SharedKernel.Behaviors.ICorrelationIdProvider, SharedKernel.Behaviors.CorrelationIdProvider>();

// Distributed Cache (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "TravelPortal:";
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// JWT Authentication for SignalR
builder.Services.AddJwtAuthentication(jwtSettings);

// SignalR with Redis backplane for horizontal scaling
builder.Services.AddSignalRNotifications(redisConnection);
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddEventBus(rabbitMqHost, "guest", "guest");

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database", tags: new[] { "db" })
    .AddRedis(redisConnection, name: "redis", tags: new[] { "cache" });

// CORS for SignalR connections
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // React Web
                "http://localhost:3001",  // React Admin
                "http://localhost:19006"  // React Native (Expo)
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();  // Required for SignalR
    });
});

var app = builder.Build();

NotificationService.Infrastructure.DependencyInjection.InitializeDatabase(connectionString);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseCors();  // Enable CORS for SignalR
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();  // Global exception handling

app.UseHttpsRedirection();
app.UseAuthentication();  // Required for SignalR [Authorize]
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

