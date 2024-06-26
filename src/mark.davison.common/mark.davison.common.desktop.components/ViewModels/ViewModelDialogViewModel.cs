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
        Content = formViewModel;
        FormSubmission = formSubmission;
    }

    public TFormViewModel Content { get; }
    public IFormSubmission<TFormViewModel> FormSubmission { get; }

    public override async Task<Response> Primary()
    {
        return await FormSubmission.Primary(Content);
    }
}

internal sealed class InnerInformationDialogViewModel
{
    public InnerInformationDialogViewModel(string primary) : this(primary, string.Empty) { }
    public InnerInformationDialogViewModel(string primary, string secondary)
    {
        PrimaryContent = primary;
        SecondaryContent = secondary;
    }

    public string PrimaryContent { get; }
    public string SecondaryContent { get; }
}

internal sealed class InformationDialogViewModel : ViewModelDialogViewModel
{
    public InformationDialogViewModel(string content)
    {
        Content = new InnerInformationDialogViewModel(content);
    }

    public InnerInformationDialogViewModel Content { get; }
    public override Task<Response> Primary() => Task.FromResult(new Response());
}