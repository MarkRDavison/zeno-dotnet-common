﻿namespace mark.davison.common.client.desktop.components.Controls;

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
        _commonApplicationNotificationService.PageChanged += OnPageChanged;
        _commonApplicationNotificationService.PageEnabledStateChanged += OnPageEnabledStateChanged;
        _commonApplicationNotificationService.PageClosed += OnPageClosed;

        if (OidcAuthenticatorViewModel is null)
        {
            SelectFirstEnabledPage();
        }
    }

    private void OnPageClosed(object? sender, ClosePageEventArgs e)
    {
        foreach (var pg in PageGroups)
        {
            if (pg.Id == e.GroupId || string.IsNullOrEmpty(e.GroupId))
            {
                foreach (var page in pg.SubPages)
                {
                    if (page.Id == e.PageId)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            var index = pg.SubPages.IndexOf(page);
                            if (index != -1)
                            {
                                pg.SubPages.Remove(page);

                                if (pg.SelectedIndex == index ||
                                    pg.SelectedIndex >= pg.SubPages.Count)
                                {
                                    pg.SelectedIndex = 0;
                                }

                                pg.SubPages[pg.SelectedIndex].Select();
                            }
                        });
                        return;
                    }
                }
            }
        }
    }

    private void OnPageEnabledStateChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            foreach (var pg in PageGroups)
            {
                pg.NotifyDisabledChange();
            }
        });
    }

    private void SelectFirstEnabledPage()
    {
        foreach (var g in PageGroups.Where(_ => !_.Disabled))
        {
            if (g.SubPages.FirstOrDefault(_ => !_.Disabled) is { } p)
            {
                SelectPage(g.Id, p.Id);
                break;
            }
        }
    }

    private void OnPageChanged(object? sender, ChangePageEventArgs e) => SelectPage(e.GroupId, e.PageId);

    private void SelectPage(string groupId, string pageId)
    {
        if (PageGroups.FirstOrDefault(_ => _.Id == groupId) is { Disabled: false } pageGroup)
        {
            var selectedGroupIndex = PageGroups.IndexOf(pageGroup);

            if (pageGroup.SubPages.FirstOrDefault(_ => _.Id == pageId) is { Disabled: false } page)
            {
                var selectedPageIndex = pageGroup.SubPages.IndexOf(page);

                Dispatcher.UIThread.Invoke(() =>
                {
                    SelectedPageGroupIndex = selectedGroupIndex;
                    pageGroup.SelectedIndex = selectedPageIndex;
                    page.Select();
                });
            }
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
                SelectFirstEnabledPage();
            }
        }
        _initialAuthenticatedRender = false;
    }

    public bool RequireAuthentication => !string.IsNullOrEmpty(_authSettings?.Value.Authority) && !_desktopAuthenticationService.IsAuthenticated && OidcAuthenticatorViewModel is not null;

    public OidcAuthenticatorViewModel? OidcAuthenticatorViewModel { get; }

    public object ActiveViewModel => RequireAuthentication ? (OidcAuthenticatorViewModel ?? throw new InvalidOperationException()) : this;

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
    private void SelectPageGroup(PageGroup group)
    {
        if (!group.Disabled)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var groupIndex = PageGroups.IndexOf(group);

                if (groupIndex != -1)
                {
                    SelectedPageGroupIndex = groupIndex;

                    group.SubPage?.Select();
                }
            });
        }
    }

    [ObservableProperty]
    private bool _navMenuOpen = true;

    [ObservableProperty]
    private ObservableObject? _appBarChildContentViewModel;

    public string Username => _desktopAuthenticationService.Username;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPageGroup))]
    private int _selectedPageGroupIndex;

    public string ApplicationTitle { get; set; }

    public PageGroup SelectedPageGroup => PageGroups[SelectedPageGroupIndex];

    public ObservableCollection<PageGroup> PageGroups { get; } = [];

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _commonApplicationNotificationService.PageClosed -= OnPageClosed;
                _commonApplicationNotificationService.PageChanged -= OnPageChanged;
                _commonApplicationNotificationService.PageEnabledStateChanged -= OnPageEnabledStateChanged;
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
