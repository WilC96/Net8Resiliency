using Shared.AotJsonSerializer.MeteoWeather;
using Shared.Models.MeteoWeather;
using System.Text.Json.Serialization;

namespace Shared.AotJsonSerializer;

public static class JsonSerializerContextRegistry
{
    private static readonly Dictionary<Type, JsonSerializerContext> _contextMapping = new()
    {
        { typeof(MeteoForecastDto), MeteoForecastJsonSerializerContext.Default},
        { typeof(HourlyUnitsDto), MeteoForecastJsonSerializerContext.Default},
        { typeof(HourlyDto), MeteoForecastJsonSerializerContext.Default}
    };

    public static JsonSerializerContext? GetContext(Type type)
    {
        _contextMapping.TryGetValue(type, out var context);

        return context;
    }
}
