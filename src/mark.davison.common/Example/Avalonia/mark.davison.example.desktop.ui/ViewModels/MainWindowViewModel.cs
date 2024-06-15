using mark.davison.common.desktop.components.Controls;
namespace mark.davison.example.desktop.ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(IServiceProvider services)
    {
        BasicApplicationViewModel = new BasicApplicationViewModel(
            "Example",
            services);

        BasicApplicationViewModel.Pages.Add(new ExamplePageViewModel("Page 1"));
        BasicApplicationViewModel.Pages.Add(new ExamplePageViewModel("Page 2"));
        BasicApplicationViewModel.Pages.Add(new ExamplePageViewModel("Page 3"));
    }

    public BasicApplicationViewModel BasicApplicationViewModel { get; }
}
