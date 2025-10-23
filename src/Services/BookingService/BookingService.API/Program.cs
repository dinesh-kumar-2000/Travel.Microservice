using BookingService.Application;
using BookingService.Infrastructure;
using EventBus.Extensions;
using Identity.Shared;
using MassTransit;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Extensions;
using SharedKernel.SignalR;
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

// Add common infrastructure (Redis, Audit, Versioning, Rate Limiting, etc.)
builder.Services.AddCommonInfrastructure(redisConnection);
builder.Services.AddMultiTenancy();
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

// SignalR for real-time notifications
builder.Services.AddSignalRNotifications(redisConnection);
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();

// Event Bus with SignalR event handlers
builder.Services.AddEventBus(rabbitMqHost, "guest", "guest");

// Health checks are already registered by AddCommonInfrastructure()

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
else
{
    app.UseHsts(); // HTTP Strict Transport Security for production
}

// Middleware pipeline
app.UseCors();  // Enable CORS
app.UseCommonMiddleware();  // Adds CorrelationId and GlobalExceptionHandling
app.UseMultiTenancy();

app.UseHttpsRedirection();
app.UseRateLimiter();
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

