﻿namespace mark.davison.common.client.desktop.components.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCommonDesktop(this IServiceCollection services)
    {
        services
            .AddLogging()
            .AddTransient<OidcAuthenticatorViewModel>()
            .AddSingleton<ICommonApplicationNotificationService, CommonApplicationNotificationService>()
            .AddSingleton<IDesktopAuthenticationService, DesktopAuthenticationService>()
            .AddSingleton<IDialogService, DialogService>();

        return services;
    }
}
