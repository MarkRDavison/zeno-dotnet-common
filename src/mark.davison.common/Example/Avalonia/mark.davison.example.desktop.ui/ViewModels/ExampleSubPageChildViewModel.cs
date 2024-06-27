namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleSubPageChildViewModel : BasicApplicationPageViewModel
{
    public ExampleSubPageChildViewModel(int number, bool canClose,
        ICommonApplicationNotificationService commonApplicationNotificationService
    ) : base(
        commonApplicationNotificationService)

    {
        Number = number;
        IsClosable = canClose;
    }

    public int Number { get; }

    public override string Name => $"Child {Number}";
    public override bool Disabled => false;
    public override bool IsClosable { get; }
}
