namespace mark.davison.common.server.sample.api;

public class AppSettings : IAppSettings
{
    public string API_ORIGIN { get; set; } = string.Empty;
    public string SECTION => "SAMPLE";
}
