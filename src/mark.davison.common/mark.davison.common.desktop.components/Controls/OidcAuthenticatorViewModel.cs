namespace mark.davison.common.client.desktop.components.Controls;

public partial class OidcAuthenticatorViewModel : ObservableObject
{
    private readonly IDesktopAuthenticationService _desktopAuthenticationService;

    public OidcAuthenticatorViewModel(
        IDesktopAuthenticationService desktopAuthenticationService)
    {
        _desktopAuthenticationService = desktopAuthenticationService;

        _desktopAuthenticationService
            .LoginFromPersisted()
            .ContinueWith(_ =>
        {
            AutoLoggingIn = false;
        });
    }

    [RelayCommand]
    private async Task Login(CancellationToken cancellationToken)
    {
        LoggingIn = true;

        var (success, loginResponse) = await _desktopAuthenticationService.LoginAsync(cancellationToken);

        Response = loginResponse;

        LoggingIn = false;
    }

    [ObservableProperty]
    private string _response = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnyLoggingIn))]
    private bool _loggingIn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnyLoggingIn))]
    private bool _autoLoggingIn = true;

    public bool AnyLoggingIn => AutoLoggingIn || LoggingIn;
}
