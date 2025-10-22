using EventBus.Extensions;
using Identity.Shared;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Auditing;
using SharedKernel.Caching;
using SharedKernel.Middleware;
using SharedKernel.RateLimiting;
using SharedKernel.Versioning;
using Tenancy;
using TenantService.Application;
using TenantService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.WithProperty("Service", "TenantService")
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
    c.SwaggerDoc("v1", new() { Title = "Tenant Service API", Version = "v1" });
    
    // JWT Bearer Authorization in Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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

// JWT Authentication
builder.Services.AddJwtAuthentication(jwtSettings);

// API Versioning
builder.Services.AddApiVersioningConfiguration();

// Rate Limiting
builder.Services.AddTenantRateLimiting();

// Multi-Tenancy Support
builder.Services.AddMultiTenancy();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
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

TenantService.Infrastructure.DependencyInjection.InitializeDatabase(connectionString);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts(); // HTTP Strict Transport Security for production
}

// Middleware
app.UseCors();  // Enable CORS
app.UseMultiTenancy();  // Enable Multi-Tenancy
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

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

app.Run();

