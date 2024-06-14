namespace mark.davison.example.shared.commands.Scenarios;

public sealed class ExampleCommandHandler : ICommandHandler<ExampleCommandRequest, ExampleCommandResponse>
{
    public Task<ExampleCommandResponse> Handle(ExampleCommandRequest command, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        return Task.FromResult(new ExampleCommandResponse
        {
            Value = command.Payload + " - and then the api added this"
        });
    }
}
