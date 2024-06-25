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

        var appNotification = services.GetRequiredService<ICommonApplicationNotificationService>();

        BasicApplicationViewModel.PageGroups.Add(new("Navigation",
            [
                new ProgrammaticNavigationPageViewModel(
                    BasicApplicationViewModel,
                    services.GetRequiredService<ICommonApplicationNotificationService>())
            ]));
        BasicApplicationViewModel.PageGroups.Add(new("Data",
            [
                new ExampleDataGridPageViewModel()
            ]));
        BasicApplicationViewModel.PageGroups.Add(new("Misc",
            [
                new ExamplePageViewModel("Page 2", appNotification)
            ]));
        BasicApplicationViewModel.PageGroups.Add(new("CQRS",
            [
                services.GetRequiredService<ExampleClientRepositoryPageViewModel>()
            ]));
        BasicApplicationViewModel.PageGroups.Add(new("Disabled",
            [
                new ExamplePageViewModel("Sub disabled", appNotification)
            ]));
        BasicApplicationViewModel.PageGroups.Add(new("Dialog",
            [
                services.GetRequiredService<ExampleDialogPageViewModel>()
            ]));
        BasicApplicationViewModel.PageGroups.Add(new("Sub Pages",
            [
                services.GetRequiredService<ExampleSubPageViewModel>(),
                new ExampleSubPageChildViewModel("2nd sub page")
            ]));
    }

    public BasicApplicationViewModel BasicApplicationViewModel { get; }
}
