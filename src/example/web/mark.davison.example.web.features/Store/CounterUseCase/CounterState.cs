namespace mark.davison.example.web.features.Store.CounterUseCase;

public sealed class CounterState : IClientState
{
    public CounterState() : this(0)
    {

    }

    public CounterState(int counterValue)
    {
        CounterValue = counterValue;
    }

    public int CounterValue { get; }
}
