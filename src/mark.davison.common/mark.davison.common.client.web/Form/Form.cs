using mark.davison.common.client.abstractions.Form;

namespace mark.davison.common.client.web.Form;

public class Form<TFormViewModel> : ComponentBase, IForm<TFormViewModel> where TFormViewModel : IFormViewModel
{
    [Parameter, EditorRequired]
    public required TFormViewModel FormViewModel { get; set; }
}
