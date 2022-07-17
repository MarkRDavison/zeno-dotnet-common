namespace mark.davison.common.client.CQRS;

public class QueryDispatcher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QueryDispatcher(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : class, IQuery<TQuery, TQueryResult>
        where TQueryResult : class, new()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
        return handler.Handle(query, cancellationToken);
    }

    public Task<TQueryResult> Dispatch<TQuery, TQueryResult>(CancellationToken cancellationToken)
        where TQuery : class, IQuery<TQuery, TQueryResult>, new()
        where TQueryResult : class, new()
    {
        return Dispatch<TQuery, TQueryResult>(new TQuery(), cancellationToken);
    }

}
