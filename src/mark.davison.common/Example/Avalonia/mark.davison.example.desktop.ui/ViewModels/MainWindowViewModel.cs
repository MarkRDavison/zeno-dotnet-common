namespace mark.davison.example.desktop.ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(IServiceProvider services)
    {
        BasicApplicationViewModel = new BasicApplicationViewModel(
            "Example",
            services)
        {
            AppBarChildContentViewModel = new AppBarChildContentViewModel()
        };

        BasicApplicationViewModel.Pages.Add(new ExampleDataGridPageViewModel());
        BasicApplicationViewModel.Pages.Add(new ExamplePageViewModel("Page 2"));
        BasicApplicationViewModel.Pages.Add(services.GetRequiredService<ExampleClientRepositoryPageViewModel>());
        BasicApplicationViewModel.Pages.Add(new ExamplePageViewModel("Disabled"));
        BasicApplicationViewModel.Pages.Add(services.GetRequiredService<ExampleDialogPageViewModel>());
    }

    public BasicApplicationViewModel BasicApplicationViewModel { get; }
}
