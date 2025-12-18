namespace mark.davison.common.Services;

public sealed class RateLimitService : IRateLimitService
{
    private readonly ConcurrencyLimiter _limiter;
    private readonly TimeSpan _cooldown;
    private readonly ConcurrentQueue<DateTime> _releaseTimes = new();

    public RateLimitService(int permitLimit, TimeSpan cooldown, int maxQueueLength = int.MaxValue)
    {
        _limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = permitLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = maxQueueLength
        });

        _cooldown = cooldown;
    }

    public async Task<IAsyncDisposable?> WaitToProceedAsync(CancellationToken ct = default)
    {
        var lease = await _limiter.AcquireAsync(1, ct);
        if (!lease.IsAcquired)
        {
            lease.Dispose();
            return null;
        }

        return new AsyncDisposableCallback("RateLimitLease", async () =>
        {
            try
            {
                await Task.Delay(_cooldown, ct);
            }
            finally
            {
                lease.Dispose();
            }
        });
    }


    public IAsyncDisposable? TryProceed()
    {
        var lease = _limiter.AttemptAcquire(1);
        if (!lease.IsAcquired)
        {
            lease.Dispose();
            return null;
        }

        return new AsyncDisposableCallback("RateLimitLease", async () =>
        {
            await Task.Delay(_cooldown);
            lease.Dispose();
        });
    }

}
