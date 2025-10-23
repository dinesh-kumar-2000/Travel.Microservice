using EventBus.Extensions;
using Identity.Shared;
using IdentityService.Application;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SendGrid;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Auditing;
using SharedKernel.Caching;
using SharedKernel.Compression;
using SharedKernel.Middleware;
using SharedKernel.RateLimiting;
using SharedKernel.Versioning;
using SharedKernel.Logging;
using SharedKernel.Tracing;
using SharedKernel.HealthChecks;
using Tenancy;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
var environment = builder.Environment.EnvironmentName;
Log.Logger = environment == "Development" 
    ? LoggingConfiguration.ConfigureDevelopmentLogging("IdentityService").CreateLogger()
    : LoggingConfiguration.ConfigureProductionLogging(
        "IdentityService",
        builder.Configuration["Elasticsearch:Url"],
        builder.Configuration["Seq:Url"]).CreateLogger();

builder.Host.UseSerilog();

// Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

var jwtSettings = new JwtSettings
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found"),
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "TravelPortal",
    Audience = builder.Configuration["Jwt:Audience"] ?? "TravelPortal",
    ExpiryMinutes = int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? "60")
};

var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Response Compression (gzip, Brotli)
builder.Services.AddResponseCompressionConfiguration();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Identity Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Correlation ID - register the provider from SharedKernel.Behaviors
builder.Services.AddScoped<SharedKernel.Behaviors.ICorrelationIdProvider, SharedKernel.Behaviors.CorrelationIdProvider>();

// Distributed Cache (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "TravelPortal:";
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();

// Multi-tenancy
builder.Services.AddMultiTenancy();

// JWT Authentication
builder.Services.AddJwtAuthentication(jwtSettings);

// API Versioning
builder.Services.AddApiVersioningConfiguration();

// Rate Limiting
builder.Services.AddTenantRateLimiting();

// Application Layer
builder.Services.AddApplication();

// Infrastructure Layer
builder.Services.AddInfrastructure(connectionString);

// Event Bus
builder.Services.AddEventBus(rabbitMqHost, rabbitMqUsername, rabbitMqPassword);

// OpenTelemetry Tracing + Prometheus Metrics
builder.Services.AddOpenTelemetryTracing(
    "IdentityService",
    "1.0.0",
    builder.Configuration["Jaeger:Host"],
    builder.Configuration["Zipkin:Url"],
    environment == "Development");

// Health Checks
builder.Services.AddComprehensiveHealthChecks(redisConnection);

// Email Service (SendGrid)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddHttpClient<ISendGridClient, SendGridClient>(client =>
{
    var apiKey = builder.Configuration["Email:SendGridApiKey"];
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
});
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// Social Login Services
builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.AddHttpClient<GoogleOAuthService>();
builder.Services.AddScoped<ISocialLoginService, GoogleOAuthService>();

// Password Reset Service
builder.Services.Configure<PasswordResetSettings>(builder.Configuration.GetSection("PasswordReset"));
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Account Lockout Service
builder.Services.Configure<AccountLockoutSettings>(builder.Configuration.GetSection("AccountLockout"));
builder.Services.AddScoped<IAccountLockoutService, AccountLockoutService>();

// Security Audit Service
builder.Services.Configure<SecurityAuditSettings>(builder.Configuration.GetSection("SecurityAudit"));
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

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

// Initialize Database
IdentityService.Infrastructure.DependencyInjection.InitializeDatabase(connectionString);

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Response Compression (must be before other middleware)
app.UseResponseCompression();

// CORS (must be before authentication)
app.UseCors();

// Custom Middleware from SharedKernel
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SharedKernel.Logging.RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Rate Limiting
app.UseRateLimiter();

app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("messaging")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

app.Run();

