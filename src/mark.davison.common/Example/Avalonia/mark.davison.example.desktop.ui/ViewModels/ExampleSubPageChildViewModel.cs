namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleSubPageChildViewModel : BasicApplicationPageViewModel
{
    public ExampleSubPageChildViewModel(string name)
    {
        Name = name;
    }

    public override string Name { get; }
    public override bool Disabled => false;
}
