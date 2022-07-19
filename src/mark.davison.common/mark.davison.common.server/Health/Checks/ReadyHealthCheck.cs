namespace mark.davison.common.server.Health.Checks;

public class ReadyHealthCheck : IHealthCheck
{
    public static string Name = "ReadyHealthCheck";

    private readonly IApplicationHealthState _applicationHealthState;

    public ReadyHealthCheck(IApplicationHealthState applicationHealthState)
    {
        _applicationHealthState = applicationHealthState;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_applicationHealthState.Ready.GetValueOrDefault())
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        return Task.FromResult(HealthCheckResult.Unhealthy());
    }
}