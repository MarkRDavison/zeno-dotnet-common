namespace mark.davison.common.client.Form;

public class Form<TFormViewModel> : ComponentBase, IForm<TFormViewModel> where TFormViewModel : IFormViewModel
{
    [Parameter, EditorRequired]
    public required TFormViewModel FormViewModel { get; set; }
}
