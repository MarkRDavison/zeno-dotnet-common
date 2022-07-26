namespace mark.davison.common.client.CQRS;

public class ActionDispatcher : IActionDispatcher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ActionDispatcher(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Dispatch<TAction>(TAction action, CancellationToken cancellationToken)
        where TAction : class, IAction<TAction>
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IActionHandler<TAction>>();
        return handler.Handle(action, cancellationToken);
    }

    public Task Dispatch<TAction>(CancellationToken cancellationToken)
        where TAction : class, IAction<TAction>, new()
    {
        return Dispatch<TAction>(new TAction(), cancellationToken);
    }

}
