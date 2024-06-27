using System.Linq;

namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleSubPageViewModel : BasicApplicationPageViewModel
{
    private int number = 3;
    private readonly ICommonApplicationNotificationService _commonApplicationNotificationService;
    private readonly BasicApplicationViewModel _basicApplicationViewModel;
    public ExampleSubPageViewModel(
        ICommonApplicationNotificationService commonApplicationNotificationService,
        BasicApplicationViewModel basicApplicationViewModel
    ) : base(
        commonApplicationNotificationService)
    {
        _commonApplicationNotificationService = commonApplicationNotificationService;
        _basicApplicationViewModel = basicApplicationViewModel;
    }


    public override string Name => "Subpage";
    public override bool Disabled => false;

    [RelayCommand]
    private void OpenChild()
    {
        var pg = _basicApplicationViewModel.PageGroups.First(_ => _.Id == "Sub Pages");

        var newPage = new ExampleSubPageChildViewModel(number++, true, _commonApplicationNotificationService);

        pg.SubPages.Add(newPage);

        _commonApplicationNotificationService.ChangePage("Sub Pages", newPage.Id);
    }
}
