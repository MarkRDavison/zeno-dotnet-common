namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public class SampleCommandHandler : ValidateAndProcessCommandHandler<SampleCommandRequest, SampleCommandResponse>
{
    public SampleCommandHandler(
        ICommandProcessor<SampleCommandRequest, SampleCommandResponse> processor,
        ICommandValidator<SampleCommandRequest, SampleCommandResponse> validator
    ) : base(
        processor,
        validator
    )
    {
    }
}
