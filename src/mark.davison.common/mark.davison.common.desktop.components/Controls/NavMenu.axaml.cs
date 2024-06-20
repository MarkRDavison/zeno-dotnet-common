namespace mark.davison.common.client.desktop.components;

public partial class NavMenu : UserControl
{
    public NavMenu()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> MenuOpenProperty =
    AvaloniaProperty.Register<AppBar, bool>(nameof(MenuOpen));

    public bool MenuOpen
    {
        get { return GetValue(MenuOpenProperty); }
        set { SetValue(MenuOpenProperty, value); }
    }

    public static readonly StyledProperty<ObservableCollection<BasicApplicationPageViewModel>> PagesProperty =
    AvaloniaProperty.Register<AppBar, ObservableCollection<BasicApplicationPageViewModel>>(nameof(Pages));

    public ObservableCollection<BasicApplicationPageViewModel> Pages
    {
        get { return GetValue(PagesProperty); }
        set { SetValue(PagesProperty, value); }
    }

    public static readonly StyledProperty<IRelayCommand<BasicApplicationPageViewModel>> SelectPageCommandProperty =
    AvaloniaProperty.Register<AppBar, IRelayCommand<BasicApplicationPageViewModel>>(nameof(SelectPageCommand));

    public IRelayCommand<BasicApplicationPageViewModel> SelectPageCommand
    {
        get { return GetValue(SelectPageCommandProperty); }
        set { SetValue(SelectPageCommandProperty, value); }
    }

    public static readonly StyledProperty<BasicApplicationPageViewModel> SelectedPageProperty =
    AvaloniaProperty.Register<AppBar, BasicApplicationPageViewModel>(nameof(SelectedPage));

    public BasicApplicationPageViewModel SelectedPage
    {
        get { return GetValue(SelectedPageProperty); }
        set { SetValue(SelectedPageProperty, value); }
    }
}