namespace mark.davison.common.client.web.abstractions.Form;

public interface IForm<TFormViewModel> where TFormViewModel : IFormViewModel
{
    TFormViewModel FormViewModel { get; set; }
}