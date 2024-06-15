namespace mark.davison.common.desktop.components.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCommonDesktop(this IServiceCollection services)
    {
        services
            .AddLogging()
            .AddTransient<OidcAuthenticatorViewModel>()
            .AddSingleton<ICommonApplicationNotificationService, CommonApplicationNotificationService>()
            .AddSingleton<IDesktopAuthenticationService, DesktopAuthenticationService>();

        return services;
    }
}
