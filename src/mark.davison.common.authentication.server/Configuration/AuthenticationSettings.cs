namespace mark.davison.common.authentication.server.Configuration;

public sealed class AuthenticationSettings : IAppSettings
{
    public string? ADMIN_EMAIL { get; set; }
    public List<AuthenticationProviderConfiguration> PROVIDERS { get; set; } = [];
}
