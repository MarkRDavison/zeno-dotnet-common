namespace mark.davison.common.client;

[ExcludeFromCodeCoverage]
public static class DependencyInversionExtensions
{
    public static IServiceCollection UseState(this IServiceCollection services)
    {
        services.AddSingleton<IStateStore, StateStore>();
        services.AddSingleton<IComponentSubscriptions, ComponentSubscriptions>();
        return services;
    }

}
