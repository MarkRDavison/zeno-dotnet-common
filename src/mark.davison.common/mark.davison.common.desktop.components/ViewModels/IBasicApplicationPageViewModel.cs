namespace mark.davison.common.client.desktop.components.ViewModels;

public interface IBasicApplicationPageViewModel
{
    string Id { get; }
    string GroupId { get; set; }
    string Name { get; }
    bool Disabled { get; }
    bool IsClosable { get; }
}
