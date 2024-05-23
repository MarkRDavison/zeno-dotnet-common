namespace mark.davison.common.server.Configuration;

public sealed class AuthenticationConfiguration
{
    public Func<User, Task>? OnUserCreated { get; set; }
}
