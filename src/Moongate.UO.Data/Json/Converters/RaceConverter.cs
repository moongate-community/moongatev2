using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Races.Base;

namespace Moongate.UO.Data.Json.Converters;

public class RaceConverter : JsonConverter<Race>
{
    public override Race? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => Race.AllRaces[reader.GetInt32()]
        };
    }

    public override void Write(Utf8JsonWriter writer, Race value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.RaceID);
    }
}
