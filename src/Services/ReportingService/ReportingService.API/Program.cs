using Identity.Shared;
using ReportingService.Application;
using ReportingService.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Auditing;
using SharedKernel.Caching;
using SharedKernel.Middleware;
using SharedKernel.RateLimiting;
using SharedKernel.Versioning;
using Tenancy;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.WithProperty("Service", "ReportingService")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

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
    c.SwaggerDoc("v1", new() { Title = "Reporting Service API", Version = "v1" });
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

builder.Services.AddMultiTenancy();
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddTenantRateLimiting();  // Add rate limiting

// Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

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

ReportingService.Infrastructure.DependencyInjection.InitializeDatabase(connectionString);

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
app.UseRateLimiter();  // Add rate limiting
app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.Run();

