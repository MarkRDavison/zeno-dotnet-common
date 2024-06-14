namespace mark.davison.example.web.components.Ignition;

public static class DependecyInjectionExtensions
{
    public static IServiceCollection AddExampleComponents(
        this IServiceCollection services,
        IAuthenticationConfig authConfig)
    {
        services.UseCommonClient(authConfig, typeof(Routes));
        services.AddWebServices();
        return services;
    }
}
