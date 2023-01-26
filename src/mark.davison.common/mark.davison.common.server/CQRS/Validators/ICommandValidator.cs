namespace mark.davison.common.server.CQRS.Validators;

public interface ICommandValidator<TRequest, TResponse>
    where TRequest : class, ICommand<TRequest, TResponse>
    where TResponse : Response, new()
{
    public Task<TResponse> ValidateAsync(TRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken);
}
