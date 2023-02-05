namespace mark.davison.common.server.sample.cqrs.Models.Commands;

[PostRequest(Path = "sample-command")]
public class SampleCommandRequest : ICommand<SampleCommandRequest, SampleCommandResponse>
{
}
