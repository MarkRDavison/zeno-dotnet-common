namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExamplePageViewModel : BasicApplicationPageViewModel
{
    public ExamplePageViewModel(string name)
    {
        Name = name;
    }

    public override string Name { get; }
    public override bool Disabled => Name.Contains("isabled");
}
