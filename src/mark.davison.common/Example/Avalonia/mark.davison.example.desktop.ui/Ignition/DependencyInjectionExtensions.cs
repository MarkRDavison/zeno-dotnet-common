﻿namespace mark.davison.example.desktop.ui.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddExampleDesktop(this IServiceCollection services)
    {
        services
            .AddCommonDesktop()
            .Configure<OdicClientSettings>(_ =>
            {
                _.Authority = "https://keycloak.markdavison.kiwi/realms/markdavison.kiwi";
                _.ClientId = "zeno-example-public";
                _.Scope = "openid profile email offline_access";
                _.PersistenceLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "example");
            })
            .AddTransient<ExampleDialogPageViewModel>()
            .AddTransient<ExampleSubPageViewModel>()
            .AddTransient<ExampleClientRepositoryPageViewModel>()
            .AddTransient<IFormSubmission<ExampleFormViewModel>, ExampleFormViewModelSubmission>();

        services.AddSingleton<IClientHttpRepository>(_ => _.GetRequiredService<IDesktopAuthenticationService>().GetAuthenticatedClient("https://localhost:50000"));

        return services;
    }
}
