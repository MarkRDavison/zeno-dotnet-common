namespace mark.davison.common.server.Configuration;

public sealed class AuthenticationConfiguration
{
    public Func<IServiceProvider, User, Task>? OnUserCreated { get; set; }
}
