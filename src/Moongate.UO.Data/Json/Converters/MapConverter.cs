using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Maps;

namespace Moongate.UO.Data.Json.Converters;

public class MapConverter : JsonConverter<Map>
{
    public override Map Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Number => Map.Maps[reader.GetInt32()],
            _                    => throw new JsonException("Value must be a number or string")
        };

    public override void Write(Utf8JsonWriter writer, Map value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.MapID);
}
