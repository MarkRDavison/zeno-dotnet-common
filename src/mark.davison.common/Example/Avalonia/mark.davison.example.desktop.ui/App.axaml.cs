using mark.davison.common.client.desktop.components.Services;

namespace mark.davison.example.desktop.ui;

public partial class App : Application
{
    private IServiceProvider _services = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Startup += OnStartup;
            desktop.Exit += OnExit;

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var collection = new ServiceCollection();

            collection.AddExampleDesktop();

            _services = collection.BuildServiceProvider();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_services),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    private void OnStartup(object? s, ControlledApplicationLifetimeStartupEventArgs e)
    {
        var logger = _services.GetRequiredService<ILogger<App>>();

        logger.LogInformation("Starting app");
    }

    private void OnExit(object? s, ControlledApplicationLifetimeExitEventArgs e)
    {
        var logger = _services.GetRequiredService<ILogger<App>>();

        logger.LogInformation("Exiting app");

        var authService = _services.GetRequiredService<IDesktopAuthenticationService>();

        authService.PersistLogin();
    }
}