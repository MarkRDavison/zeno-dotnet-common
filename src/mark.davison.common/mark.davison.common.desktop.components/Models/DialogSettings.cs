namespace mark.davison.common.client.desktop.components.Models;

public sealed class DialogSettings
{
    public double MinWidth { get; set; }
    public double MinHeight { get; set; }
    public bool CanResize { get; set; } = true;
    public bool ShowInTaskbar { get; set; } = true;
    public SizeToContent SizeToContent { get; set; } = SizeToContent.WidthAndHeight;
}
