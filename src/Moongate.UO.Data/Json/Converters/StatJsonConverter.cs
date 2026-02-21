using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Json.Converters;

public sealed class StatJsonConverter : JsonConverter<Stat>
{
    public override Stat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException("Stat value cannot be empty.");
            }

            if (TryMapString(value, out var mapped))
            {
                return mapped;
            }

            throw new JsonException($"Unsupported stat value '{value}'.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var number) && Enum.IsDefined(typeof(Stat), number))
            {
                return (Stat)number;
            }

            throw new JsonException("Unsupported numeric stat value.");
        }

        throw new JsonException("Stat value must be a string or number.");
    }

    public override void Write(Utf8JsonWriter writer, Stat value, JsonSerializerOptions options)
    {
        var text = value switch
        {
            Stat.Strength => "Str",
            Stat.Dexterity => "Dex",
            Stat.Intelligence => "Int",
            _ => value.ToString()
        };

        writer.WriteStringValue(text);
    }

    private static bool TryMapString(string value, out Stat stat)
    {
        stat = Stat.Strength;

        if (value.Equals("Str", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Strength", StringComparison.OrdinalIgnoreCase))
        {
            stat = Stat.Strength;
            return true;
        }

        if (value.Equals("Dex", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Dexterity", StringComparison.OrdinalIgnoreCase))
        {
            stat = Stat.Dexterity;
            return true;
        }

        if (value.Equals("Int", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Intelligence", StringComparison.OrdinalIgnoreCase))
        {
            stat = Stat.Intelligence;
            return true;
        }

        return false;
    }
}
