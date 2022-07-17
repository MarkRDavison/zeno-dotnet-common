namespace mark.davison.common.client.abstractions.State;

public interface IStateStore
{
    IStateInstance<TState> GetState<TState>() where TState : class, IState, new();

    void SetState<TState>(TState state) where TState : class, IState, new();

    void Reset();
}
