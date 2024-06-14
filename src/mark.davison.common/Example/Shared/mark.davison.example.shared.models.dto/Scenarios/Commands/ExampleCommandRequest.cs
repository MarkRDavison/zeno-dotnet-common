namespace mark.davison.example.shared.models.dto.Scenarios.Commands;

[PostRequest(Path = "example-command")]
public sealed class ExampleCommandRequest : ICommand<ExampleCommandRequest, ExampleCommandResponse>
{
    public string Payload { get; set; } = string.Empty;
}
