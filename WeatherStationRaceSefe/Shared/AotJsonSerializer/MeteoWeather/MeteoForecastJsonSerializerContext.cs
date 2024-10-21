using Shared.Models.MeteoWeather;
using System.Text.Json.Serialization;

namespace Shared.AotJsonSerializer.MeteoWeather;

[JsonSerializable(typeof(MeteoForecastDto))]
[JsonSerializable(typeof(HourlyUnitsDto))]
[JsonSerializable(typeof(HourlyDto))]
public partial class MeteoForecastJsonSerializerContext : JsonSerializerContext
{
}
