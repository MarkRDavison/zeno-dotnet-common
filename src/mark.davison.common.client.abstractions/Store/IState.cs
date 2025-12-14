namespace mark.davison.common.client.abstractions.Store;

public interface IState<TState> where TState : class, new()
{
    TState Value { get; }
    event EventHandler OnStateChange;
}
