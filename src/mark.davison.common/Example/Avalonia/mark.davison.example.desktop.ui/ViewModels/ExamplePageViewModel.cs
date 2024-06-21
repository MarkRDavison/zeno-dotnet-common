namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExamplePageViewModel : BasicApplicationPageViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FirstTimeText))]
    private bool _firstTime;

    public string FirstTimeText => FirstTime ? "True" : "False";

    public ExamplePageViewModel(string name)
    {
        Name = name;
    }

    public override string Name { get; }
    public override bool Disabled => Name.Contains("isabled");

    protected override void OnSelected(bool firstTime)
    {
        FirstTime = firstTime;
    }
}
