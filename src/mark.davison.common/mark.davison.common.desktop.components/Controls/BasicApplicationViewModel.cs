namespace mark.davison.common.client.desktop.components.Controls;

public partial class BasicApplicationViewModel : ObservableObject, IDisposable
{
    private bool _initialAuthenticatedRender = true;
    private bool disposedValue;
    private readonly IServiceProvider _services;
    private readonly IOptions<OdicClientSettings>? _authSettings;
    private readonly ICommonApplicationNotificationService _commonApplicationNotificationService;
    private readonly IDesktopAuthenticationService _desktopAuthenticationService;

    public BasicApplicationViewModel(string applicationTitle, IServiceProvider services)
    {
        ApplicationTitle = applicationTitle;
        _services = services;
        _authSettings = services.GetService<IOptions<OdicClientSettings>>();

        if (_authSettings is not null)
        {
            OidcAuthenticatorViewModel = services.GetRequiredService<OidcAuthenticatorViewModel>();
        }

        _commonApplicationNotificationService = services.GetRequiredService<ICommonApplicationNotificationService>();
        _desktopAuthenticationService = services.GetRequiredService<IDesktopAuthenticationService>();

        Setup();
    }

    void Setup()
    {
        _commonApplicationNotificationService.AuthenticationStateChanged += OnAuthChanged;

        if (OidcAuthenticatorViewModel is null)
        {
            _ = SelectPage(Pages[SelectedPageIndex]);
        }
    }

    private void OnAuthChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(ActiveViewModel));
        OnPropertyChanged(nameof(Username));

        if (OidcAuthenticatorViewModel is not null)
        {
            if (_initialAuthenticatedRender)
            {
                _ = SelectPage(Pages[SelectedPageIndex]);
            }
        }
        _initialAuthenticatedRender = false;
    }

    public bool RequireAuthentication => !string.IsNullOrEmpty(_authSettings?.Value.Authority) && !_desktopAuthenticationService.IsAuthenticated && OidcAuthenticatorViewModel is not null;

    public OidcAuthenticatorViewModel? OidcAuthenticatorViewModel { get; }

    public ObservableObject ActiveViewModel => RequireAuthentication ? (OidcAuthenticatorViewModel ?? throw new InvalidOperationException()) : this;

    [RelayCommand]
    private void ToggleMenu()
    {
        NavMenuOpen = !NavMenuOpen;
    }

    [RelayCommand]
    private void ManageMenu(string value)
    {
        if (value == "LOGOUT")
        {
            _desktopAuthenticationService.Logout();
        }
    }

    [RelayCommand]
    private async Task SelectPage(BasicApplicationPageViewModel viewModel)
    {
        if (!viewModel.Disabled)
        {
            await viewModel.SelectAsync();
            Dispatcher.UIThread.Invoke(() => SelectedPageIndex = Pages.IndexOf(viewModel));
        }
    }

    [ObservableProperty]
    private bool _navMenuOpen = true;

    [ObservableProperty]
    private ObservableObject? _appBarChildContentViewModel;

    public string Username => _desktopAuthenticationService.Username;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPage))]
    private int _selectedPageIndex;

    public string ApplicationTitle { get; set; }

    public BasicApplicationPageViewModel SelectedPage => Pages[SelectedPageIndex];

    public ObservableCollection<BasicApplicationPageViewModel> Pages { get; } = [];

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _commonApplicationNotificationService.AuthenticationStateChanged -= OnAuthChanged;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
