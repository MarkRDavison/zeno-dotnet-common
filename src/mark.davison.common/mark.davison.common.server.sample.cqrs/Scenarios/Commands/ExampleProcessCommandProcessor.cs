namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public sealed class ExampleProcessCommandProcessor : ICommandProcessor<ExampleProcessCommandRequest, ExampleProcessCommandResponse>
{
    public Task<ExampleProcessCommandResponse> ProcessAsync(ExampleProcessCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ExampleProcessCommandResponse { });
    }
}
