namespace mark.davison.common.client.desktop.components.Services;

public sealed class DialogService : IDialogService
{
    private IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse?> ShowDialogAsync<TResponse, TFormViewModel>(TFormViewModel viewModel)
        where TResponse : Response, new()
        where TFormViewModel : IFormViewModel
    {
        return await ShowDialogAsync<TResponse, TFormViewModel>(viewModel, new DialogSettings
        {
            CanResize = false,
            ShowInTaskbar = true,
            SizeToContent = SizeToContent.WidthAndHeight
        });
    }
    public async Task<TResponse?> ShowDialogAsync<TResponse, TFormViewModel>(TFormViewModel viewModel, DialogSettings settings)
        where TResponse : Response, new()
        where TFormViewModel : IFormViewModel
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialog = new ViewModelDialogWindow
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
                Icon = desktop.MainWindow.Icon,
                DataContext = new ViewModelDialogViewModel<TFormViewModel>(
                    viewModel,
                    _serviceProvider.GetRequiredService<IFormSubmission<TFormViewModel>>())
                {
                    ShowCancel = settings.ShowCancel,
                    CancelText = settings.CancelText,
                    PrimaryText = settings.PrimaryText
                }
            };

            return await dialog.ShowDialog<TResponse?>(desktop.MainWindow);
        }

        return new TResponse
        {
            Errors = ["Cannot retrieve MainWindow from IClassicDesktopStyleApplicationLifetime"]
        };
    }
}
