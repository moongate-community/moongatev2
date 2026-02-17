using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Json.Weather;

/// <summary>
/// Represents a weather type configuration from JSON
/// </summary>
public class JsonWeather
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Rain properties
    public int RainChance { get; set; }

    public JsonIntensityRange RainIntensity { get; set; } = new();

    public int RainTempDrop { get; set; }

    // Snow properties
    public int SnowChance { get; set; }

    public JsonIntensityRange SnowIntensity { get; set; } = new();

    public int SnowThreshold { get; set; }

    // Storm properties
    public int StormChance { get; set; }

    public JsonIntensityRange StormIntensity { get; set; } = new();

    public int StormTempDrop { get; set; }

    // Temperature properties
    public int MaxTemp { get; set; }

    public int MinTemp { get; set; }

    // Cold properties
    public int ColdChance { get; set; }

    public int ColdIntensity { get; set; }

    // Heat properties
    public int HeatChance { get; set; }

    public int HeatIntensity { get; set; }

    // Light properties (optional, may not be present in all weather types)
    public int? LightMin { get; set; }

    public int? LightMax { get; set; }

    /// <summary>
    /// Gets the temperature range for this weather type
    /// </summary>
    [JsonIgnore]
    public int TemperatureRange => MaxTemp - MinTemp;

    /// <summary>
    /// Gets the average temperature for this weather type
    /// </summary>
    [JsonIgnore]
    public double AverageTemperature => (MaxTemp + MinTemp) / 2.0;

    /// <summary>
    /// Checks if this weather type supports rain
    /// </summary>
    [JsonIgnore]
    public bool HasRain => RainChance > 0;

    /// <summary>
    /// Checks if this weather type supports snow
    /// </summary>
    [JsonIgnore]
    public bool HasSnow => SnowChance > 0;

    /// <summary>
    /// Checks if this weather type supports storms
    /// </summary>
    [JsonIgnore]
    public bool HasStorms => StormChance > 0;

    /// <summary>
    /// Checks if this is a cold climate
    /// </summary>
    [JsonIgnore]
    public bool IsColdClimate => MaxTemp <= 15;

    /// <summary>
    /// Checks if this is a hot climate
    /// </summary>
    [JsonIgnore]
    public bool IsHotClimate => MinTemp >= 20;

    /// <summary>
    /// Checks if this weather type has extreme conditions
    /// </summary>
    [JsonIgnore]
    public bool IsExtreme => ColdChance > 50 || HeatChance > 50 || StormChance > 50;

    /// <summary>
    /// Determines the current weather condition based on chances
    /// </summary>
    /// <param name="currentTemp">Current temperature</param>
    /// <param name="random">Random instance (optional)</param>
    /// <returns>Current weather condition</returns>
    public JsonWeatherCondition DetermineJsonWeatherCondition(int currentTemp, Random? random = null)
    {
        random ??= new();

        // Check for snow first (higher priority in cold weather)
        if (currentTemp <= SnowThreshold && random.Next(100) < SnowChance)
        {
            return JsonWeatherCondition.Snow;
        }

        // Check for storms
        if (random.Next(100) < StormChance)
        {
            return JsonWeatherCondition.Storm;
        }

        // Check for rain
        if (random.Next(100) < RainChance)
        {
            return JsonWeatherCondition.Rain;
        }

        return JsonWeatherCondition.Clear;
    }

    /// <summary>
    /// Gets the damage intensity for the current weather condition
    /// </summary>
    /// <param name="condition">Current weather condition</param>
    /// <param name="random">Random instance (optional)</param>
    /// <returns>Damage intensity value</returns>
    public int GetDamageIntensity(JsonWeatherCondition condition, Random? random = null)
    {
        return condition switch
        {
            JsonWeatherCondition.Rain  => RainIntensity.GetRandomValue(random),
            JsonWeatherCondition.Snow  => SnowIntensity.GetRandomValue(random),
            JsonWeatherCondition.Storm => StormIntensity.GetRandomValue(random),
            _                          => 0
        };
    }

    /// <summary>
    /// Gets the appropriate temperature drop for the current weather
    /// </summary>
    /// <param name="condition">Current weather condition</param>
    /// <returns>Temperature drop amount</returns>
    public int GetTemperatureDrop(JsonWeatherCondition condition)
    {
        return condition switch
        {
            JsonWeatherCondition.Rain  => RainTempDrop,
            JsonWeatherCondition.Storm => StormTempDrop,
            JsonWeatherCondition.Snow  => RainTempDrop, // Snow uses rain temp drop
            _                          => 0
        };
    }

    public override string ToString()
        => $"{Name} (ID: {Id}) - Temp: {MinTemp}-{MaxTemp}°C";
}
