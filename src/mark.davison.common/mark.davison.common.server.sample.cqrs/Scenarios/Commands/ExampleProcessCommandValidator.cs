namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public sealed class ExampleProcessCommandValidator : ICommandValidator<ExampleProcessCommandRequest, ExampleProcessCommandResponse>
{
    public Task<ExampleProcessCommandResponse> ValidateAsync(ExampleProcessCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ExampleProcessCommandResponse { });
    }
}
