namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public class ExamplePostCommandHandler : ICommandHandler<ExamplePostRequest, ExamplePostResponse>
{
    public async Task<ExamplePostResponse> Handle(ExamplePostRequest command, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        await Task.CompletedTask;
        return new ExamplePostResponse
        {
            TestEnumValue = command.TestEnumValue
        };
    }
}
