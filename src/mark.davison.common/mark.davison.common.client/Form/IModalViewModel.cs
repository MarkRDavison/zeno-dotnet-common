﻿namespace mark.davison.common.client.Form;

public interface IModalViewModel<TFormViewModel, TForm>
    where TFormViewModel : class, IFormViewModel, new()
    where TForm : ComponentBase
{
    TFormViewModel FormViewModel { get; set; }

    Task<Response> Primary(TFormViewModel formViewModel);
}
