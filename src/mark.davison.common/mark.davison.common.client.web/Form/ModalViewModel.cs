namespace mark.davison.common.client.web.Form;

public class ModalViewModel<TFormViewModel, TForm> : IModalViewModel<TFormViewModel, TForm>
    where TFormViewModel : class, IFormViewModel, new()
    where TForm : ComponentBase
{
    private readonly IFormSubmission<TFormViewModel> _formSubmission;

    public ModalViewModel(
        IFormSubmission<TFormViewModel> formSubmission)
    {
        _formSubmission = formSubmission;

        FormViewModel = new();
    }

    public TFormViewModel FormViewModel { get; set; }

    public async Task<Response> Primary(TFormViewModel formViewModel) => await _formSubmission.Primary(formViewModel);
}
