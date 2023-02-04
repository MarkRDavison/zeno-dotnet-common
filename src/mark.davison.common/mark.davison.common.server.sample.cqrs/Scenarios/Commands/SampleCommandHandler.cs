namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public class SampleCommandHandler : ICommandHandler<SampleCommandRequest, SampleCommandResponse>
{
    public Task<SampleCommandResponse> Handle(SampleCommandRequest command, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }
}
