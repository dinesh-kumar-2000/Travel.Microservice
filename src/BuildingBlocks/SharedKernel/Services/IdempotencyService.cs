namespace SharedKernel.Services;

public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string idempotencyKey, object? result = null, CancellationToken cancellationToken = default);
}

public class InMemoryIdempotencyService : IIdempotencyService
{
    private readonly HashSet<string> _processedKeys = new();

    public Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processedKeys.Contains(idempotencyKey));
    }

    public Task MarkAsProcessedAsync(string idempotencyKey, object? result = null, CancellationToken cancellationToken = default)
    {
        _processedKeys.Add(idempotencyKey);
        return Task.CompletedTask;
    }
}

