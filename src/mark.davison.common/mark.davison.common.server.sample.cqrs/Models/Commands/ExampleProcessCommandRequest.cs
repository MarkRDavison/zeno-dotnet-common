namespace mark.davison.common.server.sample.cqrs.Models.Commands;

[PostRequest(Path = "example-val-and-proc-command", AllowAnonymous = true)]
public sealed class ExampleProcessCommandRequest : ICommand<ExampleProcessCommandRequest, ExampleProcessCommandResponse>
{
}
