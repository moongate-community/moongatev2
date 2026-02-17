using System.Text.Json;
using System.Text.Json.Serialization;
using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Json.Converters;

public class Point3DConverter : JsonConverter<Point3D>
{
    public override Point3D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }

        int x = 0,
            y = 0,
            z = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new(x, y, z);
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
                    case "z":
                        z = reader.GetInt32();

                        break;
                    default:
                        throw new JsonException($"Unexpected property: {propertyName}");
                }
            }
        }

        throw new JsonException("Expected end of object.");
    }

    /// <summary>
    /// Read Point3D from JSON property name (dictionary key)
    /// </summary>
    public override Point3D ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return ParseFromString(value);
    }

    public override void Write(Utf8JsonWriter writer, Point3D value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Write Point3D as JSON property name (dictionary key)
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, Point3D value, JsonSerializerOptions options)
    {
        // Convert Point3D to string format for dictionary key
        var keyString = $"{value.X},{value.Y},{value.Z}";
        writer.WritePropertyName(keyString);
    }

    /// <summary>
    /// Parse Point3D from string representation
    /// </summary>
    private static Point3D ParseFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Point3D string value cannot be null or empty");
        }

        var parts = value.Split(',');

        if (parts.Length != 3)
        {
            throw new JsonException($"Invalid Point3D string format: {value}. Expected format: 'x,y,z'");
        }

        if (!int.TryParse(parts[0].Trim(), out var x))
        {
            throw new JsonException($"Invalid X coordinate in Point3D: {parts[0]}");
        }

        if (!int.TryParse(parts[1].Trim(), out var y))
        {
            throw new JsonException($"Invalid Y coordinate in Point3D: {parts[1]}");
        }

        if (!int.TryParse(parts[2].Trim(), out var z))
        {
            throw new JsonException($"Invalid Z coordinate in Point3D: {parts[2]}");
        }

        return new(x, y, z);
    }
}
