namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExamplePageViewModel : BasicApplicationPageViewModel
{
    private readonly ICommonApplicationNotificationService _commonApplicationNotificationService;
    public ExamplePageViewModel(string name, ICommonApplicationNotificationService commonApplicationNotificationService)
    {
        Name = name;
        _commonApplicationNotificationService = commonApplicationNotificationService;
    }

    protected override void OnSelected(bool firstTime)
    {
        FirstTime = firstTime;
    }

    public override string Name { get; }
    public override bool Disabled => Name.Contains("isabled") && !DisabledToggle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FirstTimeText))]
    private bool _firstTime;

    public string FirstTimeText => FirstTime ? "True" : "False";

    private bool _disabledToggle;
    public bool DisabledToggle
    {
        get => _disabledToggle;
        set
        {
            if (_disabledToggle != value)
            {
                _disabledToggle = value;
                OnPropertyChanged(nameof(DisabledToggle));
                _commonApplicationNotificationService.NotifyPageEnabledStateChanged();
            }
        }
    }
}
