namespace mark.davison.common.client.desktop.ViewModels;

internal abstract class ViewModelDialogViewModel
{
    public abstract Task<Response> Primary();
    public bool ShowCancel { get; init; } = true;
    public string CancelText { get; init; } = "Cancel";
    public string PrimaryText { get; init; } = "Save";
}

internal sealed class ViewModelDialogViewModel<TFormViewModel> : ViewModelDialogViewModel, IViewModelDialogViewModel<TFormViewModel> where TFormViewModel : IFormViewModel
{
    public ViewModelDialogViewModel(TFormViewModel formViewModel, IFormSubmission<TFormViewModel> formSubmission)
    {
        FormViewModel = formViewModel;
        FormSubmission = formSubmission;
    }

    public TFormViewModel FormViewModel { get; }
    public IFormSubmission<TFormViewModel> FormSubmission { get; }

    public override async Task<Response> Primary()
    {
        return await FormSubmission.Primary(FormViewModel);
    }
}
