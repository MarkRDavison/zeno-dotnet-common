namespace mark.davison.common.client.State;

public abstract class ComponentWithState : ComponentBase, IComponentWithState, IDisposable
{

    [ExcludeFromCodeCoverage]
    public virtual void ReRender()
    {
        InvokeAsync(StateHasChanged);
    }

    [Inject]
    public IStateStore StateStore { get; set; } = default!;

    [Inject]
    public IComponentSubscriptions ComponentSubscriptions { get; set; } = default!;

    [Inject]
    public ICQRSDispatcher Dispatcher { get; set; } = default!;

    public IStateInstance<TState> GetState<TState>() where TState : class, IState, new()
    {
        ComponentSubscriptions.Add<TState>(this);
        return StateStore.GetState<TState>();
    }

    public void SetState<TState>(TState state) where TState : class, IState, new() => StateStore.SetState(state);

    public void Dispose()
    {
        ComponentSubscriptions.Remove(this);
    }
}
