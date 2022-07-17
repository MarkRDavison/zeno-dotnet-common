namespace mark.davison.common.client.State;

public class StateInstance<TState> : IStateInstance<TState> where TState : IState
{
    public StateInstance(Func<TState> fetcher)
    {
        _fetcher = fetcher;
    }

    public TState Instance => _fetcher();

    private Func<TState> _fetcher;
}
