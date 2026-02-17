using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Json.Weather;

public class JsonIntensityRange
{
    public int Min { get; set; }

    public int Max { get; set; }

    /// <summary>
    /// Gets the average intensity value
    /// </summary>
    public double Average => (Min + Max) / 2.0;

    /// <summary>
    /// Checks if the range represents no intensity (both min and max are 0)
    /// </summary>
    [JsonIgnore]
    public bool IsZero => Min == 0 && Max == 0;

    /// <summary>
    /// Gets a random value within the intensity range
    /// </summary>
    /// <param name="random">Random instance (optional)</param>
    /// <returns>Random intensity value between min and max</returns>
    public int GetRandomValue(Random? random = null)
    {
        random ??= new();

        return random.Next(Min, Max + 1);
    }

    public override string ToString()
        => Min == Max ? $"{Min}" : $"{Min}-{Max}";
}
