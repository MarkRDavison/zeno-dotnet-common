namespace mark.davison.common.server.sample.api.Scenarios.ExampleGet;

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
