namespace mark.davison.common.client.Store;

public class Dispatcher : IDispatcher
{
    private readonly IActionSubscriber _actionSubscriber;
    private readonly IServiceProvider _services;

    public Dispatcher(
        IActionSubscriber actionSubscriber,
        IServiceProvider services)
    {
        _actionSubscriber = actionSubscriber;
        _services = services;
    }
    public async void Dispatch<TAction>(TAction action) where TAction : class, new()
    {
        StateStore.ApplyReducers(_services, action);
        _actionSubscriber.Notify(action);
        await StateStore.DispatchEffects(_services, action);
    }

}
