using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Ids;

namespace Moongate.UO.Data.Json.Converters;

public class SerialConverter : JsonConverter<Serial>
{
    public override Serial Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => Serial.Parse(reader.GetString()!),
            JsonTokenType.Number => new((uint)reader.GetInt32()),
            _                    => throw new JsonException("Invalid token type for Serial")
        };
    }

    public override void Write(Utf8JsonWriter writer, Serial value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
