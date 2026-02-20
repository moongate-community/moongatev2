using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Json.Converters;

/// <summary>
/// Converts integers from either decimal numeric tokens or hexadecimal string values.
/// </summary>
public sealed class Int32FlexibleJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => Parse(reader.GetString()),
            _                    => throw new JsonException($"Unsupported token type for int conversion: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);

    private static int Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Integer value cannot be null or empty.");
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(trimmed[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
            {
                throw new JsonException($"Invalid hexadecimal integer value: {value}");
            }

            return hexValue;
        }

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
        {
            return numericValue;
        }

        throw new JsonException($"Invalid integer value: {value}");
    }
}
