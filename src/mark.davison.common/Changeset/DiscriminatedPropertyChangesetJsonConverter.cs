namespace mark.davison.common.Changeset;

public class DiscriminatedPropertyChangesetJsonConverter : JsonConverter<DiscriminatedPropertyChangeset>
{
    public override DiscriminatedPropertyChangeset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        DiscriminatedPropertyChangeset discriminatedPropertyChangeset = new DiscriminatedPropertyChangeset();
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string json = string.Empty;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? property = reader.GetString();
            if (!reader.Read())
            {
                break;
            }

            string propertyValue = reader.GetString() ?? string.Empty;
            switch (property)
            {
                case "Name":
                    discriminatedPropertyChangeset.Name = propertyValue;
                    break;
                case "PropertyType":
                    discriminatedPropertyChangeset.PropertyType = propertyValue;
                    break;
                case "Value":
                    json = propertyValue;
                    break;
            }
        }

        if (string.IsNullOrEmpty(discriminatedPropertyChangeset.Name) || string.IsNullOrEmpty(discriminatedPropertyChangeset.PropertyType))
        {
            throw new JsonException();
        }

        var type = Type.GetType(discriminatedPropertyChangeset.PropertyType) ?? throw new JsonException();
        discriminatedPropertyChangeset.Value = JsonSerializer.Deserialize(json, type, options);
        return discriminatedPropertyChangeset;
    }

    public override void Write(Utf8JsonWriter writer, DiscriminatedPropertyChangeset value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(DiscriminatedPropertyChangeset.Name));
        writer.WriteStringValue(value.Name);
        writer.WritePropertyName(nameof(DiscriminatedPropertyChangeset.PropertyType));
        writer.WriteStringValue(value.PropertyType);
        writer.WritePropertyName(nameof(DiscriminatedPropertyChangeset.Value));
        writer.WriteStringValue(JsonSerializer.Serialize(value.Value));
        writer.WriteEndObject();
    }
}