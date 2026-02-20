using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.UO.Data.Json.Converters;

/// <summary>
/// Converts <see cref="HueSpec" /> values from numeric, hex, or <c>hue(min:max)</c> string formats.
/// </summary>
public sealed class HueSpecJsonConverter : JsonConverter<HueSpec>
{
    public override HueSpec Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => HueSpec.FromValue(reader.GetInt32()),
            JsonTokenType.String => Parse(reader.GetString()),
            _                    => throw new JsonException($"Unsupported token type for HueSpec: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, HueSpec value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());

    private static HueSpec Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("HueSpec cannot be null or empty.");
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("hue(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(')'))
        {
            var content = trimmed[4..^1];
            var parts = content.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var min) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var max))
            {
                throw new JsonException($"Invalid hue range format: {value}");
            }

            return HueSpec.FromRange(min, max);
        }

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(trimmed[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
            {
                throw new JsonException($"Invalid hexadecimal hue value: {value}");
            }

            return HueSpec.FromValue(hexValue);
        }

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
        {
            return HueSpec.FromValue(numericValue);
        }

        throw new JsonException($"Invalid hue value: {value}");
    }
}
