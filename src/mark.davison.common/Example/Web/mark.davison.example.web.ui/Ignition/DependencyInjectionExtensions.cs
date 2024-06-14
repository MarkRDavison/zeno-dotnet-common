namespace mark.davison.example.web.ui.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddExampleWeb(this IServiceCollection services, IAuthenticationConfig authConfig)
    {
        services
            .AddExampleComponents(authConfig)
            .UseFluxorState(typeof(Program), typeof(FeaturesRootType))
            .UseClientRepository(WebConstants.ApiClientName, WebConstants.LocalBffRoot);

        return services;
    }
}
