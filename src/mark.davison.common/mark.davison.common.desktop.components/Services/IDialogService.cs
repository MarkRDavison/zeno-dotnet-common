namespace mark.davison.common.client.desktop.components.Services;

public interface IDialogService
{
    Task<TResponse?> ShowDialogAsync<TResponse, TDialogViewModel>(TDialogViewModel viewModel)
        where TResponse : Response, new()
        where TDialogViewModel : IViewModelDialogViewModel;
    Task<TResponse?> ShowDialogAsync<TResponse, TDialogViewModel>(TDialogViewModel viewModel, DialogSettings settings)
        where TResponse : Response, new()
        where TDialogViewModel : IViewModelDialogViewModel;
}
