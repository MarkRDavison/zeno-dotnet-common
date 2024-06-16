namespace mark.davison.common.desktop.components.Services;

public sealed class DialogService : IDialogService
{
    public async Task<TResponse?> ShowDialogAsync<TResponse, TDialogViewModel>(TDialogViewModel viewModel)
        where TResponse : Response, new()
        where TDialogViewModel : IViewModelDialogViewModel
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialog = new ViewModelDialogWindow
            {
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 400,
                MinHeight = 300,
                CanResize = false,
                SystemDecorations = SystemDecorations.Full,
                ShowInTaskbar = true,
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
