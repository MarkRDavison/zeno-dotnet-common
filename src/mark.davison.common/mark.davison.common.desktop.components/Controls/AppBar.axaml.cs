namespace mark.davison.common.desktop.components;

public partial class AppBar : UserControl
{
    public AppBar()
    {
        InitializeComponent();
        FlyoutMenuItems.Add(new FlyoutMenuItem { Name = "Logout", Value = "LOGOUT" });
    }

    public static readonly StyledProperty<IRelayCommand> MenuToggleCommandProperty =
        AvaloniaProperty.Register<AppBar, IRelayCommand>(nameof(MenuToggleCommand));

    public IRelayCommand MenuToggleCommand
    {
        get => GetValue(MenuToggleCommandProperty);
        set => SetValue(MenuToggleCommandProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> ManageMenuCommandProperty =
        AvaloniaProperty.Register<AppBar, IRelayCommand>(nameof(ManageMenuCommand));

    public IRelayCommand ManageMenuCommand
    {
        get => GetValue(ManageMenuCommandProperty);
        set => SetValue(ManageMenuCommandProperty, value);
    }

    public static readonly StyledProperty<string> ApplicationTitleProperty =
        AvaloniaProperty.Register<AppBar, string>(nameof(ApplicationTitle));

    public string ApplicationTitle
    {
        get => GetValue(ApplicationTitleProperty);
        set => SetValue(ApplicationTitleProperty, value);
    }

    public static readonly StyledProperty<string> UsernameProperty =
        AvaloniaProperty.Register<AppBar, string>(nameof(Username));

    public string Username
    {
        get => GetValue(UsernameProperty);
        set => SetValue(UsernameProperty, value);
    }

    public ObservableCollection<FlyoutMenuItem> FlyoutMenuItems { get; } = [];
}