namespace mark.davison.common.server.sample.cqrs.Scenarios.Queries;

public sealed class ExampleValidateAndProcessQueryProcessor : IQueryProcessor<ExampleValidateAndProcessQueryRequest, ExampleValidateAndProcessQueryResponse>
{
    public Task<ExampleValidateAndProcessQueryResponse> ProcessAsync(ExampleValidateAndProcessQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        return Task<ExampleValidateAndProcessQueryResponse>.FromResult(new ExampleValidateAndProcessQueryResponse
        {
            Warnings = ["SUCCESS"]
        });
    }
}
