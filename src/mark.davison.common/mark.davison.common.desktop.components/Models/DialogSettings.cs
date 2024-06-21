namespace mark.davison.common.client.desktop.components.Models;

public sealed class DialogSettings
{
    public string Title { get; set; } = string.Empty;
    public double MinWidth { get; set; }
    public double MinHeight { get; set; }
    public bool CanResize { get; set; } = true;
    public bool ShowInTaskbar { get; set; } = true;
    public bool ShowCancel { get; set; } = true;
    public string CancelText { get; init; } = "Cancel";
    public string PrimaryText { get; init; } = "Save";
    public SizeToContent SizeToContent { get; set; } = SizeToContent.WidthAndHeight;
}
