namespace mark.davison.common.server.sample.cqrs.Scenarios.Queries;

public class SampleQueryHandler : IQueryHandler<SampleQueryRequest, SampleQueryResponse>
{
    public Task<SampleQueryResponse> Handle(SampleQueryRequest query, ICurrentUserContext currentUserContext, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }
}
