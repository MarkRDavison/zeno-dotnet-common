namespace mark.davison.common.server.CQRS.Processors;

public interface IQueryProcessor<TRequest, TResponse>
    where TRequest : class, IQuery<TRequest, TResponse>
    where TResponse : Response, new()
{
    public Task<TResponse> ProcessAsync(TRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken);
}
