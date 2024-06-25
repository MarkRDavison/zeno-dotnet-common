namespace mark.davison.common.client.desktop.components.ViewModels;

public abstract partial class BasicApplicationPageViewModel : ObservableObject, IBasicApplicationPageViewModel
{
    private bool _firstSelection = true;

    public void Select()
    {
        OnSelected(_firstSelection);
        _ = OnSelectedAsync(_firstSelection);
        _firstSelection = false;
    }

    protected virtual void OnSelected(bool firstTime) { }
    protected virtual Task OnSelectedAsync(bool firstTime) => Task.CompletedTask;

    public virtual string Id => Name;
    public abstract string Name { get; }
    public abstract bool Disabled { get; }
}
