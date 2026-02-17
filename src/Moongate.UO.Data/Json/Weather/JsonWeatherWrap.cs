namespace Moongate.UO.Data.Json.Weather;

public class JsonWeatherWrap
{
    public JsonDfnHeader Header { get; set; }
    public List<JsonWeather> WeatherTypes { get; set; } = new();
}
