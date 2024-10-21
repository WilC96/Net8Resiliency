using Shared.Core;
using System.Text.Json.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace Shared.Models.MeteoWeather;

public record MeteoForecastDto(
    [property: JsonPropertyName("latitude")] double? Latitude,
    [property: JsonPropertyName("longitude")] double? Longitude,
    [property: JsonPropertyName("generationtime_ms")] double? GenerationtimeMs,
    [property: JsonPropertyName("utc_offset_seconds")] int? UtcOffsetSeconds,
    [property: JsonPropertyName("timezone")] string? Timezone,
    [property: JsonPropertyName("timezone_abbreviation")] string? TimezoneAbbreviation,
    [property: JsonPropertyName("elevation")] double? Elevation,
    [property: JsonPropertyName("hourly_units")] HourlyUnitsDto? HourlyUnits,
    [property: JsonPropertyName("hourly")] HourlyDto? Hourly,
    [property: JsonIgnore] WeatherStation? Station);

public record HourlyUnitsDto(
    [property: JsonPropertyName("time")] string? Time,
    [property: JsonPropertyName("temperature_2m")] string? Temperature2m);

public record HourlyDto(
    [property: JsonPropertyName("time")] List<string>? Time,
    [property: JsonPropertyName("temperature_2m")] List<double>? Temperature2m);
