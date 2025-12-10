namespace mark.davison.common.authentication.server.Configuration;

public sealed class AuthenticationProviderConfiguration
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? Authority { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string[]? Scope { get; init; }
}
