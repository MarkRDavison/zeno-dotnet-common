namespace mark.davison.example.web.components.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddExampleComponents(this IServiceCollection services)
    {
        services.UseClientRepository(WebConstants.ApiClientName, WebConstants.LocalBffRoot);
        services.UseAuthentication(WebConstants.ApiClientName, WebConstants.LocalBffRoot);
        services.UseClientCQRS(typeof(Routes));
        services.UseCommonClient();
        services.AddClientState();

        return services;
    }
}
