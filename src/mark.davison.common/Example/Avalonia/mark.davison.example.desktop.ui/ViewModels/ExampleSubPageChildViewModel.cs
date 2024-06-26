namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleSubPageChildViewModel : BasicApplicationPageViewModel
{
    public ExampleSubPageChildViewModel(string name, bool canClose,
        ICommonApplicationNotificationService commonApplicationNotificationService
    ) : base(
        commonApplicationNotificationService)

    {
        Name = name;
        IsClosable = canClose;
    }

    public override string Name { get; }
    public override bool Disabled => false;
    public override bool IsClosable { get; }
}
