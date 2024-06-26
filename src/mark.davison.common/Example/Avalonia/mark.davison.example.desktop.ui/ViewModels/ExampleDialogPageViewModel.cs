namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleDialogPageViewModel : BasicApplicationPageViewModel
{
    private readonly IDialogService _dialogService;

    public ExampleDialogPageViewModel(
        IDialogService dialogService,
        ICommonApplicationNotificationService commonApplicationNotificationService
    ) : base(
        commonApplicationNotificationService)
    {
        _dialogService = dialogService;
    }

    public override string Name => "Standard submit";

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
        Response = await _dialogService.ShowDialogAsync<Response, ExampleFormViewModel>(
            new ExampleFormViewModel(),
            new DialogSettings { Title = "Example form", PrimaryText = "Submit" });
    }

    [RelayCommand]
    private async Task OpenInformationDialog()
    {
        await _dialogService.ShowInformationDialogAsync(
            "Hello world here is some information",
            new DialogSettings
            {
                Title = "Information",
                PrimaryText = "Ok",
                ShowCancel = false
            });
    }
}
