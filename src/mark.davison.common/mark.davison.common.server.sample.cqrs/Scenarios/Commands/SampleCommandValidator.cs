namespace mark.davison.common.server.sample.cqrs.Scenarios.Commands;

public class SampleCommandValidator : ICommandValidator<SampleCommandRequest, SampleCommandResponse>
{
    public Task<SampleCommandResponse> ValidateAsync(SampleCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
