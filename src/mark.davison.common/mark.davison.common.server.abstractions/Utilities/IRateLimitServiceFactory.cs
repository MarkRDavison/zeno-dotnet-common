namespace mark.davison.common.server.abstractions.Utilities;

public interface IRateLimitServiceFactory
{
    IRateLimitService CreateRateLimiter(TimeSpan delay);
}
