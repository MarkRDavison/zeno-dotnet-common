namespace mark.davison.common.server.Health.Checks;

public class StartupHealthCheck : IHealthCheck
{
    public static string Name = nameof(StartupHealthCheck);

    private readonly IApplicationHealthState _applicationHealthState;

    public StartupHealthCheck(IApplicationHealthState applicationHealthState)
    {
        _applicationHealthState = applicationHealthState;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_applicationHealthState.Started.GetValueOrDefault())
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        return Task.FromResult(HealthCheckResult.Unhealthy());
    }
}