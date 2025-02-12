namespace mark.davison.common.server.sample.cqrs.Scenarios.Queries;

public sealed class ExampleValidateAndProcessQueryValidator : IQueryValidator<ExampleValidateAndProcessQueryRequest, ExampleValidateAndProcessQueryResponse>
{
    public Task<ExampleValidateAndProcessQueryResponse> ValidateAsync(ExampleValidateAndProcessQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ExampleValidateAndProcessQueryResponse { });
    }
}
