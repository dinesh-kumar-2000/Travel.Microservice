using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;

namespace SharedKernel.Compression;

public static class CompressionExtensions
{
    private const CompressionLevel DefaultCompressionLevel = CompressionLevel.Fastest;

    /// <summary>
    /// Configures compression provider options with a standard compression level
    /// </summary>
    private static void ConfigureCompressionLevel<T>(
        IServiceCollection services, 
        CompressionLevel level = DefaultCompressionLevel) where T : class
    {
        if (typeof(T) == typeof(BrotliCompressionProviderOptions))
        {
            services.Configure<BrotliCompressionProviderOptions>(options => 
                options.Level = level);
        }
        else if (typeof(T) == typeof(GzipCompressionProviderOptions))
        {
            services.Configure<GzipCompressionProviderOptions>(options => 
                options.Level = level);
        }
    }

    /// <summary>
    /// Add response compression (gzip and Brotli)
    /// </summary>
    public static IServiceCollection AddResponseCompressionConfiguration(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            
            // Compress these MIME types
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/xml",
                "text/plain",
                "text/csv",
                "text/html",
                "application/javascript",
                "text/css"
            });
        });

        // Configure compression levels using shared logic
        ConfigureCompressionLevel<BrotliCompressionProviderOptions>(services);
        ConfigureCompressionLevel<GzipCompressionProviderOptions>(services);

        return services;
    }
}

