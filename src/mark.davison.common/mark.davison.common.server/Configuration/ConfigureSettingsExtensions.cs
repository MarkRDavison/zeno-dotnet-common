namespace mark.davison.common.server.Configuration;

public static class ConfigureSettingsExtensions
{
    public static TSettings ConfigureSettingsServices<TSettings>(
        this IServiceCollection services,
        IConfiguration configuration
    ) where TSettings : class, IAppSettings, new()
    {
        var appSettings = new TSettings();
        var configured = configuration.GetSection(appSettings.SECTION);

        services.Configure<TSettings>(configured);
        configured.Bind(appSettings);

        RecursivelyBindAndConfigureSettings(services, configuration, typeof(TSettings), new[] { appSettings.SECTION });

        return appSettings;
    }

    private static void RecursivelyBindAndConfigureSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        Type settingType,
        IEnumerable<string> sections)
    {
        if (sections.Count() > 1)
        {
            var configured = configuration.GetSection(string.Join(":", sections));
            var methodInfo = typeof(ConfigureSettingsExtensions).GetMethod(nameof(ConfigureSettingsExtensions.ConfigureAndBind), BindingFlags.NonPublic | BindingFlags.Static)!;
            var method = methodInfo.MakeGenericMethod(settingType);
            method.Invoke(null, new object[] { services, configured });
        }

        foreach (var property in settingType.GetProperties())
        {
            if (property.PropertyType.IsAssignableTo(typeof(IAppSettings)))
            {
                var nextLevel = Activator.CreateInstance(property.PropertyType) as IAppSettings;
                if (nextLevel != null)
                {
                    RecursivelyBindAndConfigureSettings(services, configuration, property.PropertyType, sections.Append(nextLevel.SECTION));
                }
            }
        }
    }

    private static TSettings ConfigureAndBind<TSettings>(IServiceCollection services, IConfigurationSection section) where TSettings : class, IAppSettings, new()
    {
        var settings = new TSettings();

        services.Configure<TSettings>(section);
        section.Bind(settings);

        return settings;
    }
}
