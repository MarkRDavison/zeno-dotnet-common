﻿namespace mark.davison.example.desktop.ui.ViewModels;

public partial class ExampleSubPageViewModel : BasicApplicationPageViewModel
{
    public ExampleSubPageViewModel(ICommonApplicationNotificationService commonApplicationNotificationService) : base(commonApplicationNotificationService)
    {
    }

    public override string Name => "Subpage";
    public override bool Disabled => false;
}
