namespace mark.davison.common.client.Store;

public abstract class StateImplementation
{
    public abstract void SetState(object state);
    public abstract object StateValue { get; }
}

public sealed class StateImplementation<TState> : StateImplementation, IState<TState>
    where TState : class, IClientState, new()
{
    public StateImplementation()
    {
        StateStore.RegisterStateChangeCallback<TState>(Notify);
    }

    public TState Value => StateStore.GetState<TState>();
    public override object StateValue => Value;

    public override void SetState(object state)
    {
        if (state is TState typedState)
        {
            StateStore.SetState<TState>(typedState);
        }
    }

    public void Notify()
    {
        OnStateChange?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler OnStateChange = default!;
}