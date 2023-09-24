namespace mark.davison.common.server.EventDriven;

public class ChangesetQueueCron : CronJobService
{
    private readonly ILogger _logger;
    private readonly IChangesetQueue _queue;

    public ChangesetQueueCron(
        IScheduleConfig<ChangesetQueueCron> config,
        ILogger<ChangesetQueueCron> logger,
        IChangesetQueue queue
    ) : base(
        config.CronExpression,
        config.TimeZoneInfo
    )
    {
        _logger = logger;
        _queue = queue;
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        if (_queue.HasPendingBarrier())
        {
            using (_logger.ProfileOperation(LogLevel.Information, "Processing changeset queue"))
            {
                await _queue.ProcessNextBarrier();
            }
        }
        else
        {
            _logger.LogTrace("No changesets in queue to process");
        }
    }
}
