namespace mark.davison.example.desktop.ui.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddExampleDesktop(this IServiceCollection services)
    {
        services
            .AddCommonDesktop()
            .Configure<OdicClientSettings>(_ =>
            {
                _.Authority = "https://keycloak.markdavison.kiwi/auth/realms/markdavison.kiwi";
                _.ClientId = "zeno-example-public";
                _.Scope = "openid profile email offline_access zeno-example-public";
                _.PersistenceLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "example");
            });

        return services;
    }
}
