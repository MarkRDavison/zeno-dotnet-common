using mark.davison.common.client.desktop.Enums;
using mark.davison.common.client.desktop.Models;

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

    public static readonly StyledProperty<Breakpoint> BreakpointProperty =
    AvaloniaProperty.Register<AppBar, Breakpoint>(nameof(Breakpoint));

    public Breakpoint Breakpoint
    {
        get { return GetValue(BreakpointProperty); }
        set { SetValue(BreakpointProperty, value); }
    }

    private void ContentControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
        {
            var breakpoint = Breakpoint.XXL;

            if (e.NewSize.Width < BreakpointConfiguration.ExtraSmall)
            {
                breakpoint = Breakpoint.XS;
            }
            else if (e.NewSize.Width < BreakpointConfiguration.Small)
            {
                breakpoint = Breakpoint.S;
            }
            else if (e.NewSize.Width < BreakpointConfiguration.Medium)
            {
                breakpoint = Breakpoint.MD;
            }
            else if (e.NewSize.Width < BreakpointConfiguration.Large)
            {
                breakpoint = Breakpoint.LG;
            }
            else if (e.NewSize.Width < BreakpointConfiguration.ExtraLarge)
            {
                breakpoint = Breakpoint.XL;
            }

            if (Breakpoint != breakpoint)
            {
                Breakpoint = breakpoint;
                // TODO: CloseMenu???
            }
        }
    }
}