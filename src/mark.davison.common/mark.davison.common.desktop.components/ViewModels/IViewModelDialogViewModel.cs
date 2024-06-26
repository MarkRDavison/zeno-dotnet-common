namespace mark.davison.common.client.desktop.ViewModels;

public interface IViewModelDialogViewModel<TFormViewModel> where TFormViewModel : IFormViewModel
{
    TFormViewModel Content { get; }
    IFormSubmission<TFormViewModel> FormSubmission { get; }
}
