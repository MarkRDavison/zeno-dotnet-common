using System.Collections;

namespace mark.davison.common.server.abstractions.Configuration;

public static class AppSettingsExtensions
{
    public static string DumpAppSettings(this IAppSettings settings, bool safe)
    {
        var builder = new StringBuilder();

        DumpAppSettingsRecursively(settings, safe, builder, 0);

        return builder.ToString();
    }

    private static void DumpAppSettingsRecursively(IAppSettings settings, bool safe, StringBuilder builder, int depth)
    {
        var type = settings.GetType();

        builder.AppendLine(new string(' ', depth * 4) + type.GetProperty(nameof(IAppSettings.SECTION))!.GetValue(settings) + ":");

        foreach (var property in type.GetProperties())
        {
            if (property.Name == nameof(IAppSettings.SECTION)) { continue; }
            if (!property.CanRead || !property.CanWrite) { continue; }
            if (property.PropertyType.IsAssignableTo(typeof(IAppSettings)))
            {
                if (property.GetValue(settings) is IAppSettings appSettings)
                {
                    DumpAppSettingsRecursively(appSettings, safe, builder, depth + 1);
                }
            }
            else
            {
                var secret = safe && property.CustomAttributes.Any(_ => _.AttributeType == typeof(AppSettingSecretAttribute));
                var propertyValue = property.GetValue(settings);

                var value = propertyValue?.ToString() ?? string.Empty;

                if (!(value is string) && value is IEnumerable enumerable)
                {
                    value = string.Empty;
                    foreach (var en in enumerable)
                    {
                        if (value != string.Empty)
                        {
                            value += ", ";
                        }
                        value += en.ToString();
                    }
                }

                if (secret)
                {
                    value = new string('*', value.Length);
                }

                builder.AppendLine(new string(' ', (depth + 1) * 4) + property.Name + ": " + value);
            }
        }
    }
}
