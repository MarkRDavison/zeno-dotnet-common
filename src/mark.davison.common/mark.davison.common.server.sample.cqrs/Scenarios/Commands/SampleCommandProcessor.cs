namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public class SampleCommandProcessor : ICommandProcessor<SampleCommandRequest, SampleCommandResponse>
{
    public Task<SampleCommandResponse> ProcessAsync(SampleCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
