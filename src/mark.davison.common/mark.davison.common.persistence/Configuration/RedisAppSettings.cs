namespace mark.davison.common.persistence.Configuration;

[ExcludeFromCodeCoverage]
public class RedisAppSettings : IAppSettings
{
    public string SECTION => "REDIS";
    public string HOST { get; set; } = string.Empty;
    public int PORT { get; set; } = 6379;
    public string? PASSWORD { get; set; }
}
