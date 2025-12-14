namespace mark.davison.example.web.features.Store.CounterUseCase;

public static class CounterReducers
{
    [ReducerMethod]
    public static CounterState IncreaseCounterAction(CounterState state, IncreaseAction action)
    {
        return new CounterState(state.CounterValue + action.Amount);
    }

    [ReducerMethod]
    public static CounterState DecreaseCounterAction(CounterState state, DecreaseAction action)
    {
        return new CounterState(state.CounterValue - action.Amount);
    }
}