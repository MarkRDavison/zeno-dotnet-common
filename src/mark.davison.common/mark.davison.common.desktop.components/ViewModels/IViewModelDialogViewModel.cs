namespace mark.davison.common.client.desktop.ViewModels;

public interface IViewModelDialogViewModel<TFormViewModel> where TFormViewModel : IFormViewModel
{
    TFormViewModel FormViewModel { get; }
    IFormSubmission<TFormViewModel> FormSubmission { get; }
}
