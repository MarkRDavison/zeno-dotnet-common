namespace mark.davison.common.client.abstractions.State;

public interface IStateInstance<TState> where TState : IState
{
    public TState Instance { get; }
}
