namespace mark.davison.common.client.abstractions.State;

public interface IComponentWithState
{
    void ReRender();
    IStateInstance<TState> GetState<TState>() where TState : class, IState, new();
    void SetState<TState>(TState state) where TState : class, IState, new();
}
