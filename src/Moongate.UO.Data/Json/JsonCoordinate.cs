using System.Text.Json.Serialization;
using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Json;

/// <summary>
/// Represents a coordinate in the JSON format (x1, y1, x2, y2)
/// </summary>
public class JsonCoordinate
{
    [JsonPropertyName("x1")]
    public int X1 { get; set; }

    [JsonPropertyName("y1")]
    public int Y1 { get; set; }

    [JsonPropertyName("x2")]
    public int X2 { get; set; }

    [JsonPropertyName("y2")]
    public int Y2 { get; set; }

    /// <summary>
    /// Gets the width of the coordinate rectangle
    /// </summary>
    [JsonIgnore]
    public int Width => Math.Abs(X2 - X1);

    /// <summary>
    /// Gets the height of the coordinate rectangle
    /// </summary>
    [JsonIgnore]
    public int Height => Math.Abs(Y2 - Y1);

    /// <summary>
    /// Gets the area of the coordinate rectangle
    /// </summary>
    [JsonIgnore]
    public int Area => Width * Height;

    /// <summary>
    /// Checks if a point is within this coordinate rectangle
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if the point is within the rectangle</returns>
    public bool Contains(int x, int y)
    {
        var minX = Math.Min(X1, X2);
        var maxX = Math.Max(X1, X2);
        var minY = Math.Min(Y1, Y2);
        var maxY = Math.Max(Y1, Y2);

        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }

    /// <summary>
    /// Creates a JsonCoordinate from a Rectangle2D
    /// </summary>
    /// <param name="rect">The Rectangle2D to convert</param>
    /// <returns>JsonCoordinate with x1, y1, x2, y2</returns>
    public static JsonCoordinate FromRectangle2D(Rectangle2D rect)
        => new()
        {
            X1 = rect.X,
            Y1 = rect.Y,
            X2 = rect.X + rect.Width,
            Y2 = rect.Y + rect.Height
        };

    /// <summary>
    /// Converts this coordinate to a Rectangle2D
    /// </summary>
    /// <returns>Rectangle2D with x, y, width, height</returns>
    public Rectangle2D ToRectangle2D()
    {
        var x = Math.Min(X1, X2);
        var y = Math.Min(Y1, Y2);
        var width = Math.Abs(X2 - X1);
        var height = Math.Abs(Y2 - Y1);

        return new(x, y, width, height);
    }

    public override string ToString()
        => $"({X1},{Y1}) -> ({X2},{Y2}) [{Width}x{Height}]";
}
