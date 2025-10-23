using System.Diagnostics.Metrics;

namespace SharedKernel.Metrics;

/// <summary>
/// Centralized metrics collection for business and technical metrics
/// </summary>
public class MetricsCollector
{
    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _errorCounter;
    private readonly Counter<long> _businessEventCounter;
    private readonly Histogram<double> _databaseOperationDuration;
    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;

    public MetricsCollector(string serviceName)
    {
        _meter = new Meter(serviceName, "1.0.0");
        
        // Request metrics
        _requestCounter = _meter.CreateCounter<long>(
            "http_requests_total",
            "Total number of HTTP requests");
        
        _requestDuration = _meter.CreateHistogram<double>(
            "http_request_duration_seconds",
            "Duration of HTTP requests in seconds");
        
        _errorCounter = _meter.CreateCounter<long>(
            "errors_total",
            "Total number of errors");
        
        // Business metrics
        _businessEventCounter = _meter.CreateCounter<long>(
            "business_events_total",
            "Total number of business events");
        
        // Database metrics
        _databaseOperationDuration = _meter.CreateHistogram<double>(
            "database_operation_duration_seconds",
            "Duration of database operations in seconds");
        
        // Cache metrics
        _cacheHitCounter = _meter.CreateCounter<long>(
            "cache_hits_total",
            "Total number of cache hits");
        
        _cacheMissCounter = _meter.CreateCounter<long>(
            "cache_misses_total",
            "Total number of cache misses");
    }

    /// <summary>
    /// Records an HTTP request
    /// </summary>
    public void RecordHttpRequest(string method, string endpoint, int statusCode, double durationSeconds)
    {
        _requestCounter.Add(1, new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("status_code", statusCode.ToString()));
        
        _requestDuration.Record(durationSeconds, new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("status_code", statusCode.ToString()));
    }

    /// <summary>
    /// Records an error
    /// </summary>
    public void RecordError(string errorType, string errorCode)
    {
        _errorCounter.Add(1, new KeyValuePair<string, object?>("error_type", errorType),
            new KeyValuePair<string, object?>("error_code", errorCode));
    }

    /// <summary>
    /// Records a business event
    /// </summary>
    public void RecordBusinessEvent(string eventType, string? tenantId = null)
    {
        _businessEventCounter.Add(1, new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("tenant_id", tenantId ?? "unknown"));
    }

    /// <summary>
    /// Records a database operation
    /// </summary>
    public void RecordDatabaseOperation(string operationType, string tableName, double durationSeconds)
    {
        _databaseOperationDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation_type", operationType),
            new KeyValuePair<string, object?>("table_name", tableName));
    }

    /// <summary>
    /// Records a cache hit
    /// </summary>
    public void RecordCacheHit(string cacheType, string keyPattern)
    {
        _cacheHitCounter.Add(1, new KeyValuePair<string, object?>("cache_type", cacheType),
            new KeyValuePair<string, object?>("key_pattern", keyPattern));
    }

    /// <summary>
    /// Records a cache miss
    /// </summary>
    public void RecordCacheMiss(string cacheType, string keyPattern)
    {
        _cacheMissCounter.Add(1, new KeyValuePair<string, object?>("cache_type", cacheType),
            new KeyValuePair<string, object?>("key_pattern", keyPattern));
    }

    /// <summary>
    /// Creates a custom counter
    /// </summary>
    public Counter<T> CreateCounter<T>(string name, string description) 
        where T : struct
    {
        return _meter.CreateCounter<T>(name, description);
    }

    /// <summary>
    /// Creates a custom histogram
    /// </summary>
    public Histogram<T> CreateHistogram<T>(string name, string description) 
        where T : struct
    {
        return _meter.CreateHistogram<T>(name, description);
    }

    /// <summary>
    /// Creates a custom gauge
    /// </summary>
    public ObservableGauge<T> CreateGauge<T>(string name, string description, Func<T> valueProvider) 
        where T : struct
    {
        return _meter.CreateObservableGauge<T>(name, valueProvider, description);
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}
