namespace mark.davison.common.desktop.components.Services;

public sealed class DialogService : IDialogService
{
    public async Task<TResponse?> ShowDialogAsync<TResponse, TDialogViewModel>(TDialogViewModel viewModel)
        where TResponse : Response, new()
        where TDialogViewModel : IViewModelDialogViewModel
    {
        return await ShowDialogAsync<TResponse, TDialogViewModel>(viewModel, new DialogSettings
        {
            CanResize = false,
            ShowInTaskbar = true,
            SizeToContent = SizeToContent.WidthAndHeight
        });
    }
    public async Task<TResponse?> ShowDialogAsync<TResponse, TDialogViewModel>(TDialogViewModel viewModel, DialogSettings settings)
        where TResponse : Response, new()
        where TDialogViewModel : IViewModelDialogViewModel
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialog = new ViewModelDialogWindow
            {
                SizeToContent = settings.SizeToContent,
                MinWidth = settings.MinWidth,
                MinHeight = settings.MinHeight,
                CanResize = settings.CanResize,
                ShowInTaskbar = settings.ShowInTaskbar,
                SystemDecorations = SystemDecorations.Full,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ExtendClientAreaToDecorationsHint = true,
                Icon = desktop.MainWindow.Icon,
                DataContext = viewModel
            };

            return await dialog.ShowDialog<TResponse?>(desktop.MainWindow);
        }

        return new TResponse
        {
            Errors = ["Cannot retrieve MainWindow from IClassicDesktopStyleApplicationLifetime"]
        };
    }
}
