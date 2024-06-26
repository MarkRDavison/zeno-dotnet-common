namespace mark.davison.common.client.desktop.components.Services;

public interface IDialogService
{
    Task<TResponse?> ShowDialogAsync<TResponse, TFormViewModel>(TFormViewModel viewModel)
        where TResponse : Response, new()
        where TFormViewModel : IFormViewModel;
    Task<TResponse?> ShowDialogAsync<TResponse, TFormViewModel>(TFormViewModel viewModel, DialogSettings settings)
        where TResponse : Response, new()
        where TFormViewModel : IFormViewModel;

    Task ShowInformationDialogAsync(string content);
    Task ShowInformationDialogAsync(string content, DialogSettings settings);
}
