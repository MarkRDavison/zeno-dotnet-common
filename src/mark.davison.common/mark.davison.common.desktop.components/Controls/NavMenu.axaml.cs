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

    public static readonly StyledProperty<ObservableCollection<PageGroup>> PageGroupsProperty =
    AvaloniaProperty.Register<AppBar, ObservableCollection<PageGroup>>(nameof(PageGroups));

    public ObservableCollection<PageGroup> PageGroups
    {
        get { return GetValue(PageGroupsProperty); }
        set { SetValue(PageGroupsProperty, value); }
    }

    public static readonly StyledProperty<IRelayCommand<PageGroup>> SelectPageGroupCommandProperty =
    AvaloniaProperty.Register<AppBar, IRelayCommand<PageGroup>>(nameof(SelectPageGroupCommand));

    public IRelayCommand<PageGroup> SelectPageGroupCommand
    {
        get { return GetValue(SelectPageGroupCommandProperty); }
        set { SetValue(SelectPageGroupCommandProperty, value); }
    }

    public static readonly StyledProperty<PageGroup> SelectedPageGroupProperty =
    AvaloniaProperty.Register<AppBar, PageGroup>(nameof(SelectedPageGroup));

    public PageGroup SelectedPageGroup
    {
        get { return GetValue(SelectedPageGroupProperty); }
        set { SetValue(SelectedPageGroupProperty, value); }
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