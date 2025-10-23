using ReviewsService.Application;
using ReviewsService.Infrastructure;
using Identity.Shared;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.Extensions;
using Tenancy;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.WithProperty("Service", "ReviewsService")
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
    c.SwaggerDoc("v1", new() { Title = "Reviews Service API", Version = "v1" });
});

// Add common infrastructure (Redis, Audit, Versioning, Rate Limiting, etc.)
builder.Services.AddCommonInfrastructure(redisConnection);
builder.Services.AddMultiTenancy();
builder.Services.AddJwtAuthentication(jwtSettings);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

// Health checks are already registered by AddCommonInfrastructure()

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultiTenancy();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
