namespace mark.davison.example.api.Configuration;

public sealed class AppSettings : IAppSettings
{
    public string SECTION => "EXAMPLE";
    public List<AuthAppSettings> AUTH { get; set; } = new();
    public DatabaseAppSettings DATABASE { get; set; } = new();
    public RedisAppSettings REDIS { get; set; } = new();
    public ClaimsAppSettings CLAIMS { get; set; } = new(); // TODO: This needs to be a child of the auth settings
    public bool PRODUCTION_MODE { get; set; }
}