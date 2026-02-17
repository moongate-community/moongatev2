using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Professions;

namespace Moongate.UO.Data.Json.Converters;

public class ProfessionInfoConverter : JsonConverter<ProfessionInfo>
{
    public override ProfessionInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ProfessionInfo.GetProfession(reader.GetInt32(), out var profession);

        return profession;
    }

    public override void Write(Utf8JsonWriter writer, ProfessionInfo value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ID);
    }
}
