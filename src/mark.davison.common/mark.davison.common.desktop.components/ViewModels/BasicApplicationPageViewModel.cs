namespace mark.davison.common.desktop.components.ViewModels;

public abstract partial class BasicApplicationPageViewModel : ObservableObject, IBasicApplicationPageViewModel
{
    public abstract string Name { get; }
    public abstract bool Disabled { get; }
}
