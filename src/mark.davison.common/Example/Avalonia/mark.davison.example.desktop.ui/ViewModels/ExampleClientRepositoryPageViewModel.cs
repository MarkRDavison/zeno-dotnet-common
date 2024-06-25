namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleClientRepositoryPageViewModel : BasicApplicationPageViewModel
{
    private readonly IClientHttpRepository _clientHttpRepository;

    public ExampleClientRepositoryPageViewModel(IClientHttpRepository clientHttpRepository)
    {
        _clientHttpRepository = clientHttpRepository;
    }

    protected override async void OnSelected(bool firstTime)
    {
        var request = new ExampleCommandRequest
        {
            Payload = firstTime ? "First time!" : "Not the first time"
        };

        var response = await _clientHttpRepository.Post<ExampleCommandResponse, ExampleCommandRequest>(
            request,
            CancellationToken.None);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Request = request.Payload;
            Response = response.Value;
        });
    }

    [ObservableProperty]
    private string? _request = "Not sent yet";

    [ObservableProperty]
    private string? _response = "Not sent yet";

    public override string Name => "Select";
    public override bool Disabled => false;
}
