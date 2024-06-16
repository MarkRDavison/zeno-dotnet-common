namespace mark.davison.common.desktop.components.Services;

public interface IDialogService
{
    Task<TResponse?> ShowDialogAsync<TResponse, TDialogViewModel>(TDialogViewModel viewModel)
        where TResponse : Response, new()
        where TDialogViewModel : IViewModelDialogViewModel;
}
