namespace mark.davison.common.client.desktop.components.Services;

public sealed class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DialogSettings _defaultDialogSettings;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _defaultDialogSettings = new DialogSettings
        {
            CanResize = false,
            ShowInTaskbar = true,
            SizeToContent = SizeToContent.WidthAndHeight
        };
    }

    public async Task<TResponse?> ShowDialogAsync<TResponse, TFormViewModel>(TFormViewModel viewModel)
        where TResponse : Response, new()
        where TFormViewModel : IFormViewModel
    {
        return await ShowDialogAsync<TResponse, TFormViewModel>(viewModel, _defaultDialogSettings);
    }

    public async Task<TResponse?> ShowDialogAsync<TResponse, TFormViewModel>(TFormViewModel viewModel, DialogSettings settings)
        where TResponse : Response, new()
        where TFormViewModel : IFormViewModel
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialog = CreateWindow(
                new ViewModelDialogViewModel<TFormViewModel>(
                    viewModel,
                    _serviceProvider.GetRequiredService<IFormSubmission<TFormViewModel>>())
                {
                    ShowCancel = settings.ShowCancel,
                    CancelText = settings.CancelText,
                    PrimaryText = settings.PrimaryText
                },
                desktop.MainWindow.Icon, settings);

            return await dialog.ShowDialog<TResponse?>(desktop.MainWindow);
        }

        return new TResponse
        {
            Errors = ["Cannot retrieve MainWindow from IClassicDesktopStyleApplicationLifetime"]
        };
    }

    private ViewModelDialogWindow CreateWindow(object dataContext, WindowIcon? icon, DialogSettings settings)
    {
        return new ViewModelDialogWindow
        {
            Title = settings.Title,
            SizeToContent = settings.SizeToContent,
            MinWidth = settings.MinWidth,
            MinHeight = settings.MinHeight,
            CanResize = settings.CanResize,
            ShowInTaskbar = settings.ShowInTaskbar,
            SystemDecorations = SystemDecorations.Full,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ExtendClientAreaToDecorationsHint = false,
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome,
            Icon = icon,
            DataContext = dataContext
        };
    }

    public Task ShowInformationDialogAsync(string content) => ShowInformationDialogAsync(content, _defaultDialogSettings);
    public async Task ShowInformationDialogAsync(string content, DialogSettings settings)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var informationDialogViewModel = new InformationDialogViewModel(content)
            {
                ShowCancel = settings.ShowCancel,
                CancelText = settings.CancelText,
                PrimaryText = settings.PrimaryText
            };
            var dialog = CreateWindow(informationDialogViewModel, desktop.MainWindow.Icon, settings);

            await dialog.ShowDialog(desktop.MainWindow);
        }
    }
}
