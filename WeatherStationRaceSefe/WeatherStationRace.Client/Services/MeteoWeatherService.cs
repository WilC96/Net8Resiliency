using Serilog;
using Shared.Core;
using Shared.Models.MeteoWeather;

namespace WeatherStationRace.Client.Services;

public interface IMeteoWeatherService
{
    public Task<MeteoForecastDto?> GetForecastAsync(
        WeatherStation landmark,
        CancellationToken ct);
}

public sealed class MeteoWeatherService : IMeteoWeatherService
{
    private readonly IMeteoWeatherForecastClient _meteoWeatherForecastClient;

    private static readonly Dictionary<WeatherStation, (double latitude, double longitude)> _coordinates = new()
    {
        { WeatherStation.BuckinghamPalace, (51.50148, -0.14172) },
        { WeatherStation.TowerBridge, (51.50342, -0.07613) },
        { WeatherStation.BigBen, (51.49892, -0.12765) },
        { WeatherStation.BritishMuseum, (51.51902, -0.12618) },
        { WeatherStation.TrafalgarSquare, (51.50798, -0.12690) },
        { WeatherStation.Shoreditch, (51.5245, -0.0722) },
        { WeatherStation.HydePark, (51.5074, -0.1729) },
        { WeatherStation.TheShard, (51.5042, -0.0859) },
        { WeatherStation.CamdenMarket, (51.5368, -0.1455) },
        { WeatherStation.GreenwichObservatory, (51.4774, -0.0027) }
    };

    public MeteoWeatherService(
        IMeteoWeatherForecastClient meteoWeatherForecastClient)
    {
        _meteoWeatherForecastClient = meteoWeatherForecastClient;
    }

    public async Task<MeteoForecastDto?> GetForecastAsync(
        WeatherStation station,
        CancellationToken ct)
    {
        MeteoForecastDto? result = null;

        var (lat, lon) = GetCoordinates(station);

        var query = new Dictionary<string, string>
        {
            {"hourly", "temperature_2m"},
            {"latitude", lat.ToString()},
            {"longitude", lon.ToString()}
        };

        try
        {
            var response = await _meteoWeatherForecastClient.InvokeAsync<MeteoForecastDto?>(
                HttpMethod.Get,
                "v1/forecast",
                ct,
                query);

            if (response is not null) 
            {
                result = response with { Station = station };
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(GetForecastAsync)} Error. {ex.Message}");
        }

        return result;
    }

    private static (double latitude, double longitude) GetCoordinates(WeatherStation station)
    {
        if (_coordinates.TryGetValue(station, out var coords))
        {
            return coords;
        }
        throw new ArgumentException("Weather Station does not exist.");
    }
}
