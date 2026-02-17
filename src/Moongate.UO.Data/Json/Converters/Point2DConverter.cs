using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Json.Converters;

public class Point2DConverter : JsonConverter<Point2D>
{
    public override Point2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }

        int x = 0,
            y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new(x, y);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "x":
                        x = reader.GetInt32();

                        break;
                    case "y":
                        y = reader.GetInt32();

                        break;
                    default:
                        throw new JsonException($"Unexpected property: {propertyName}");
                }
            }
        }

        throw new JsonException("Expected end of object.");
    }

    /// <summary>
    /// Read Point2D from JSON property name (dictionary key)
    /// </summary>
    public override Point2D ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return ParseFromString(value);
    }

    public override void Write(Utf8JsonWriter writer, Point2D value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Write Point2D as JSON property name (dictionary key)
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, Point2D value, JsonSerializerOptions options)
    {
        // Convert Point2D to string format for dictionary key
        var keyString = $"{value.X},{value.Y}";
        writer.WritePropertyName(keyString);
    }

    /// <summary>
    /// Parse Point2D from string representation
    /// </summary>
    private static Point2D ParseFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Point2D string value cannot be null or empty");
        }

        var parts = value.Split(',');

        if (parts.Length != 2)
        {
            throw new JsonException($"Invalid Point2D string format: {value}. Expected format: 'x,y'");
        }

        if (!int.TryParse(parts[0].Trim(), out var x))
        {
            throw new JsonException($"Invalid X coordinate in Point2D: {parts[0]}");
        }

        if (!int.TryParse(parts[1].Trim(), out var y))
        {
            throw new JsonException($"Invalid Y coordinate in Point2D: {parts[1]}");
        }

        return new(x, y);
    }
}
