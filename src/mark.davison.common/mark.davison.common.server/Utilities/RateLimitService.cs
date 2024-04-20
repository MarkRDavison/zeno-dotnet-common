namespace mark.davison.common.server.Utilities;

public class RateLimitService : IRateLimitService
{
    private DateTime _next;
    private readonly IDateService _dateService;

    public RateLimitService(
        TimeSpan delay,
        IDateService dateService)
    {
        _dateService = dateService;
        Delay = delay;
        _next = _dateService.Now.Add(Delay);
    }

    public async Task<bool> Wait(CancellationToken cancellationToken)
    {
        var limited = false;

        if (_dateService.Now < _next)
        {
            await Task.Delay(_dateService.Now.Add(Delay) - _next, cancellationToken);
            limited = true;
        }

        _next = _dateService.Now.Add(Delay);

        return limited;
    }

    public TimeSpan Delay { get; }
}
