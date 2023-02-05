namespace mark.davison.common.server.sample.cqrs.Scenarios.Queries;

public class ExampleGetCommandHandler : IQueryHandler<ExampleGetRequest, ExampleGetResponse>
{
    public Task<ExampleGetResponse> Handle(ExampleGetRequest query, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        return Task.FromResult(new ExampleGetResponse
        {
            ResponseValue = query.RequestValue,
            DateOnlyValue = query.DateOnlyValue
        });
    }
}
