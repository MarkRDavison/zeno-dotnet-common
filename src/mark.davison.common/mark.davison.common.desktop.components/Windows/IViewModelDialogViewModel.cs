namespace mark.davison.common.client.desktop.components.Windows;

public interface IViewModelDialogViewModel
{
    string Title { get; }
    bool IsValid { get; }
    Task<Response> PrimaryCallback();
}
