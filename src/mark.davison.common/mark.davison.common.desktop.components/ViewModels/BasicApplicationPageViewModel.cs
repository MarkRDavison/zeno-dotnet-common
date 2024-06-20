namespace mark.davison.common.client.desktop.components.ViewModels;

public abstract partial class BasicApplicationPageViewModel : ObservableObject, IBasicApplicationPageViewModel
{
    public abstract string Name { get; }
    public abstract bool Disabled { get; }
}
