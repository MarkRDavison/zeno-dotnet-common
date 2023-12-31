namespace mark.davison.common.Changeset;

public class DiscriminatedPropertyChangesetJsonConverter : JsonConverter<DiscriminatedPropertyChangeset>
{
    public override DiscriminatedPropertyChangeset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        DiscriminatedPropertyChangeset cs = new();

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }



        string serializedValueString = string.Empty;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            // Get the key.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();

            if (!reader.Read())
            {
                break;
            }

            var val = reader.GetString() ?? string.Empty;

            switch (propertyName)
            {
                case nameof(DiscriminatedPropertyChangeset.Name):
                    cs.Name = val;
                    break;
                case nameof(DiscriminatedPropertyChangeset.PropertyType):
                    cs.PropertyType = val;
                    break;
                case nameof(DiscriminatedPropertyChangeset.Value):
                    serializedValueString = val;
                    break;
            }
        }

        if (string.IsNullOrEmpty(cs.Name) ||
            string.IsNullOrEmpty(cs.PropertyType))
        {
            throw new JsonException();
        }

        var type = Type.GetType(cs.PropertyType);

        if (type == null)
        {
            throw new JsonException();
        }

        cs.Value = JsonSerializer.Deserialize(serializedValueString, type, options);

        return cs;
    }

    public override void Write(Utf8JsonWriter writer, DiscriminatedPropertyChangeset value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(value.Name));
        writer.WriteStringValue(value.Name);
        writer.WritePropertyName(nameof(value.PropertyType));
        writer.WriteStringValue(value.PropertyType);
        writer.WritePropertyName(nameof(value.Value));
        writer.WriteStringValue(JsonSerializer.Serialize(value.Value));
        writer.WriteEndObject();
    }
}
