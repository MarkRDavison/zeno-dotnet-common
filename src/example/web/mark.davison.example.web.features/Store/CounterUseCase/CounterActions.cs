namespace mark.davison.example.web.features.Store.CounterUseCase;

public sealed class IncreaseAction : BaseAction
{
    public int Amount { get; set; }
}

public sealed class DecreaseAction : BaseAction
{
    public int Amount { get; set; }
}