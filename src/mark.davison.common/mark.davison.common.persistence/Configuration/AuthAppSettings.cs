namespace mark.davison.common.persistence.Configuration;

[ExcludeFromCodeCoverage]
public class AuthAppSettings : IAppSettings
{
    public string SECTION => "AUTH";

    public string AUTHORITY { get; set; } = string.Empty;
    public string CLIENT_ID { get; set; } = string.Empty;
    public string CLIENT_SECRET { get; set; } = string.Empty;
    public string SESSION_NAME { get; set; } = string.Empty;
    public string SCOPE { get; set; } = string.Empty;
    public List<string> SCOPES => SCOPE.Split(" ", StringSplitOptions.RemoveEmptyEntries).Where(_ => _ != null).ToList();
}
