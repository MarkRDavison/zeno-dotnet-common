namespace mark.davison.common;

public static class SerializationHelpers
{
    public static JsonSerializerOptions CreateStandardSerializationOptions() =>
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
}
