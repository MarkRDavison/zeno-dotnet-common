namespace mark.davison.common.client.Components;

public partial class ClickableIcon
{
    [Parameter, EditorRequired]
    public required string ActiveIcon { get; set; }

    [Parameter, EditorRequired]
    public required string InactiveIcon { get; set; }

    [Parameter, EditorRequired]
    public required string ActiveColour { get; set; }

    [Parameter, EditorRequired]
    public required string InactiveColour { get; set; }

    [Parameter]
    public string? ActiveTooltip { get; set; }

    [Parameter]
    public string? InactiveTooltip { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<bool> OnClick { get; set; }

    [Parameter]
    public bool Value { get; set; }

    private string _activeStyle => $"fill: {ActiveColour}";
    private string _inactiveStyle => $"fill: {(_hovered ? ActiveColour : InactiveColour)}";

    private bool _hovered;

    private void OnHoverEnter()
    {
        _hovered = true;
    }

    private void OnHoverExit()
    {
        _hovered = false;
    }

    private async Task OnClickIcon(bool setActive)
    {
        await OnClick.InvokeAsync(setActive);
        Value = setActive;
        await InvokeAsync(StateHasChanged);
    }
}
