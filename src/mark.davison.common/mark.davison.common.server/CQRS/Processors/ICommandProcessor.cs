namespace mark.davison.common.server.CQRS.Processors;

public interface ICommandProcessor<TRequest, TResponse>
    where TRequest : class, ICommand<TRequest, TResponse>
    where TResponse : Response, new()
{
    public Task<TResponse> ProcessAsync(TRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken);
}
