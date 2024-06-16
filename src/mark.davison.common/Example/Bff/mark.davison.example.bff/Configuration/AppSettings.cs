namespace mark.davison.example.bff.Configuration;

public class AppSettings : IAppSettings
{
    public string SECTION => "EXAMPLE";

    public AuthAppSettings AUTH { get; set; } = new();
    public RedisAppSettings REDIS { get; set; } = new();
    public ClaimsAppSettings CLAIMS { get; set; } = new();
    public string WEB_ORIGIN { get; set; } = "https://localhost:8080";
    public string BFF_ORIGIN { get; set; } = "https://localhost:40000";
    public string API_ORIGIN { get; set; } = "https://localhost:50000";
    public bool PRODUCTION_MODE { get; set; }
}