using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Utilities;

namespace SharedKernel.Behaviors;

public class CorrelationIdBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<CorrelationIdBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public CorrelationIdBehavior(
        ILogger<CorrelationIdBehavior<TRequest, TResponse>> logger,
        ICorrelationIdProvider correlationIdProvider)
    {
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdProvider.CorrelationId;
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestType"] = typeof(TRequest).Name
        }))
        {
            _logger.LogInformation("Processing request {RequestType} with CorrelationId {CorrelationId}", 
                typeof(TRequest).Name, correlationId);
            
            var response = await next();
            
            _logger.LogInformation("Completed request {RequestType} with CorrelationId {CorrelationId}", 
                typeof(TRequest).Name, correlationId);
            
            return response;
        }
    }
}

public interface ICorrelationIdProvider
{
    string CorrelationId { get; set; }
}

public class CorrelationIdProvider : ICorrelationIdProvider
{
    private string? _correlationId;

    public string CorrelationId
    {
        get => _correlationId ??= DefaultProviders.IdGenerator.Generate();
        set => _correlationId = value;
    }
}

