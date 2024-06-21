namespace mark.davison.common.client.desktop.components.ViewModels;

public abstract partial class BasicApplicationPageViewModel : ObservableObject, IBasicApplicationPageViewModel
{
    private bool _firstSelection = true;


    public async Task SelectAsync()
    {
        OnSelected(_firstSelection);
        await OnSelectedAsync(_firstSelection);
        _firstSelection = false;
    }

    protected virtual void OnSelected(bool firstTime) { }
    protected virtual Task OnSelectedAsync(bool firstTime) => Task.CompletedTask;

    public abstract string Name { get; }
    public abstract bool Disabled { get; }
}
