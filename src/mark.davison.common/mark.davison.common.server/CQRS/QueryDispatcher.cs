﻿namespace mark.davison.common.server.CQRS;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QueryDispatcher(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery Query, CancellationToken cancellation)
        where TQuery : class, IQuery<TQuery, TQueryResult>, new()
        where TQueryResult : class, new()
    {
        var handler = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
        var currentUserContext = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<ICurrentUserContext>();
        return handler.Handle(Query, currentUserContext, cancellation);
    }
}