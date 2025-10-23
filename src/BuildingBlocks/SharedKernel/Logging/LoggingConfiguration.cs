using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using Serilog.Sinks.Seq;
using System.Reflection;

namespace SharedKernel.Logging;

/// <summary>
/// Centralized logging configuration for all services
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with comprehensive logging setup
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(
        string serviceName,
        string environment,
        string? elasticsearchUrl = null,
        string? seqUrl = null,
        string? logFilePath = null)
    {
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Version", GetAssemblyVersion())
            .Enrich.FromLogContext();

        // Console output with structured JSON in production
        if (environment == "Development")
        {
            configuration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }
        else
        {
            configuration.WriteTo.Console(new CompactJsonFormatter());
        }

        // File logging
        if (!string.IsNullOrEmpty(logFilePath))
        {
            configuration.WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(logFilePath, $"{serviceName}-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB
                rollOnFileSizeLimit: true);
        }

        // Elasticsearch logging
        if (!string.IsNullOrEmpty(elasticsearchUrl))
        {
            configuration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                IndexFormat = $"travel-portal-{serviceName.ToLower()}-{environment.ToLower()}-{{0:yyyy.MM.dd}}",
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                FailureCallback = e => Console.WriteLine($"Unable to submit event {e.MessageTemplate}"),
                MinimumLogEventLevel = LogEventLevel.Information
            });
        }

        // Seq logging
        if (!string.IsNullOrEmpty(seqUrl))
        {
            configuration.WriteTo.Seq(seqUrl, 
                restrictedToMinimumLevel: LogEventLevel.Information);
        }

        return configuration;
    }

    /// <summary>
    /// Configures logging for development environment
    /// </summary>
    public static LoggerConfiguration ConfigureDevelopmentLogging(string serviceName)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", "Development")
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                $"logs/{serviceName}-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);
    }

    /// <summary>
    /// Configures logging for production environment
    /// </summary>
    public static LoggerConfiguration ConfigureProductionLogging(
        string serviceName,
        string? elasticsearchUrl = null,
        string? seqUrl = null)
    {
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", "Production")
            .Enrich.FromLogContext()
            .WriteTo.Console(new CompactJsonFormatter());

        // Elasticsearch logging
        if (!string.IsNullOrEmpty(elasticsearchUrl))
        {
            configuration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                IndexFormat = $"travel-portal-{serviceName.ToLower()}-prod-{{0:yyyy.MM.dd}}",
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                MinimumLogEventLevel = LogEventLevel.Information
            });
        }

        // Seq logging
        if (!string.IsNullOrEmpty(seqUrl))
        {
            configuration.WriteTo.Seq(seqUrl, 
                restrictedToMinimumLevel: LogEventLevel.Information);
        }

        return configuration;
    }

    private static string GetAssemblyVersion()
    {
        try
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
        }
        catch
        {
            return "1.0.0.0";
        }
    }
}
