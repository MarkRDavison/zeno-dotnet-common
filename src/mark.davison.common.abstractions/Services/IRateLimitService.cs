namespace mark.davison.common.abstractions.Services;

public interface IRateLimitService
{
    Task<IAsyncDisposable?> WaitToProceedAsync(CancellationToken cancellationToken);
    IAsyncDisposable? TryProceed();
}