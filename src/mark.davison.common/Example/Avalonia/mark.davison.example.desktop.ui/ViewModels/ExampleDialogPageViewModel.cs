using mark.davison.common.desktop.components.Services;

namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleDialogPageViewModel : BasicApplicationPageViewModel
{
    private readonly IDialogService _dialogService;

    public ExampleDialogPageViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public override string Name => "Dialogs";
    public override bool Disabled => false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResponseErrors))]
    [NotifyPropertyChangedFor(nameof(ResponseWarnings))]
    [NotifyPropertyChangedFor(nameof(ResponseStatus))]
    private Response? _response;

    public string ResponseErrors => Response is not null
        ? string.Join(", ", Response.Errors)
        : string.Empty;
    public string ResponseWarnings => Response is not null
        ? string.Join(", ", Response.Warnings)
        : string.Empty;
    public string ResponseStatus => Response is not null && Response.Success
        ? "Success"
        : string.Empty;

    [RelayCommand]
    private async Task OpenDialog()
    {
        Response = await _dialogService.ShowDialogAsync<Response, ExampleDialogViewModel>(
            new ExampleDialogViewModel());
    }
}
