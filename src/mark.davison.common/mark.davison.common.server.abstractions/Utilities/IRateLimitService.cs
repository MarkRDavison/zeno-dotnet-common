namespace mark.davison.common.server.abstractions.Utilities;

public interface IRateLimitService
{
    Task<bool> Wait(CancellationToken cancellationToken);
    TimeSpan Delay { get; }
}
