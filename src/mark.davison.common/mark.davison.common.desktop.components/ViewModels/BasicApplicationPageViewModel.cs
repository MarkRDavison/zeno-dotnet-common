namespace mark.davison.common.client.desktop.components.ViewModels;

public abstract partial class BasicApplicationPageViewModel : ObservableObject, IBasicApplicationPageViewModel
{
    private readonly ICommonApplicationNotificationService _commonApplicationNotificationService;

    protected BasicApplicationPageViewModel(ICommonApplicationNotificationService commonApplicationNotificationService)
    {
        _commonApplicationNotificationService = commonApplicationNotificationService;
    }

    private bool _firstSelection = true;

    public void Select()
    {
        OnSelected(_firstSelection);
        _ = OnSelectedAsync(_firstSelection);
        _firstSelection = false;
    }

    protected virtual void OnSelected(bool firstTime) { }
    protected virtual Task OnSelectedAsync(bool firstTime) => Task.CompletedTask;


    protected virtual bool OnClose() => true;

    [RelayCommand(CanExecute = nameof(CanClose))]
    private void Close()
    {
        if (CanClose() && OnClose())
        {
            _commonApplicationNotificationService.ClosePage(GroupId, Id);
        }
    }
    protected virtual bool CanClose() => true;

    public virtual string Id => Name;
    public string GroupId { get; set; } = string.Empty;
    public abstract string Name { get; }
    public abstract bool Disabled { get; }
    public virtual bool IsClosable => false;
}
