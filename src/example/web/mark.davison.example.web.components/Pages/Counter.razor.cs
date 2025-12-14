namespace mark.davison.example.web.components.Pages;

[StateProperty<CounterState>]
public partial class Counter : StateComponent
{
    [Inject]
    public required IStoreHelper StoreHelper { get; set; }

    private void Increase(int amount)
    {
        StoreHelper.Dispatch(new IncreaseAction { Amount = amount });
    }

    private void Decrease(int amount)
    {
        StoreHelper.Dispatch(new DecreaseAction { Amount = amount });
    }
}
