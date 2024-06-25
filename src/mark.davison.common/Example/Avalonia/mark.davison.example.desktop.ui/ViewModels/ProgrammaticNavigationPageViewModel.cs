using System.Collections.Generic;
using System.Linq;

namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ProgrammaticNavigationPageViewModel : BasicApplicationPageViewModel
{
    private readonly BasicApplicationViewModel _basicApplicationViewModel;
    private readonly ICommonApplicationNotificationService _commonApplicationNotificationService;

    public ProgrammaticNavigationPageViewModel(
        BasicApplicationViewModel basicApplicationViewModel,
        ICommonApplicationNotificationService commonApplicationNotificationService)
    {
        _basicApplicationViewModel = basicApplicationViewModel;
        _commonApplicationNotificationService = commonApplicationNotificationService;
    }

    public override string Name => "Nav";
    public override bool Disabled => false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Pages))]
    [NotifyCanExecuteChangedFor(nameof(NavigateCommand))]
    private string? _selectedPageGroup;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateCommand))]
    private string? _selectedPage;

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Navigate()
    {
        _commonApplicationNotificationService.ChangePage(SelectedPageGroup!, SelectedPage!);
    }

    private bool CanNavigate()
    {
        return !string.IsNullOrEmpty(SelectedPageGroup) && !string.IsNullOrEmpty(SelectedPage);
    }

    [RelayCommand]
    private void ToggleDisabledToggle()
    {
        var disabledPage = _basicApplicationViewModel.PageGroups.SelectMany(_ => _.SubPages).FirstOrDefault(_ => _.Name.Contains("isabled"));

        if (disabledPage is ExamplePageViewModel examplePage)
        {
            examplePage.DisabledToggle = !examplePage.DisabledToggle;
        }
    }

    public List<string> PageGroups => [.. _basicApplicationViewModel.PageGroups.Select(_ => _.Id)];
    public List<string> Pages => _basicApplicationViewModel.PageGroups.FirstOrDefault(_ => _.Id == SelectedPageGroup)?.SubPages.Select(_ => _.Id).ToList() ?? [];

}
