using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.UO.Data.Json.Converters;

/// <summary>
/// Converts <see cref="GoldValueSpec" /> values from numeric or <c>dice(...)</c> formats.
/// </summary>
public sealed class GoldValueSpecJsonConverter : JsonConverter<GoldValueSpec>
{
    public override GoldValueSpec Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => GoldValueSpec.FromValue(reader.GetInt32()),
            JsonTokenType.String => Parse(reader.GetString()),
            _                    => throw new JsonException($"Unsupported token type for GoldValueSpec: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, GoldValueSpec value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());

    private static GoldValueSpec Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("GoldValueSpec cannot be null or empty.");
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("dice(", StringComparison.OrdinalIgnoreCase) &&
            trimmed.EndsWith(')'))
        {
            var expression = trimmed[5..^1].Trim();

            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new JsonException($"Invalid dice expression in gold value: {value}");
            }

            return GoldValueSpec.FromDiceExpression(expression);
        }

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
        {
            return GoldValueSpec.FromValue(numericValue);
        }

        throw new JsonException($"Invalid gold value: {value}");
    }
}
