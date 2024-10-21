using Polly.CircuitBreaker;
using Serilog;
using Shared.Core;
using Shared.Models.MeteoWeather;
using WeatherStationRace.Client.Services;

namespace WeatherStationRace;

public interface IWorker
{
    public Task<string> RunAsync(CancellationTokenSource cts);
}

public sealed class Worker : IWorker
{
    private readonly IMeteoWeatherService _weatherService;

    public Worker(
        IMeteoWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<string> RunAsync(CancellationTokenSource cts)
    {
        var weatherTasks = new List<Task<MeteoForecastDto?>>();

        int canceledTasksCount = 0;
        MeteoForecastDto? fastestResult = null;

        foreach (WeatherStation station in Enum.GetValues(typeof(WeatherStation)))
        {
            var task = _weatherService.GetForecastAsync(station, cts.Token);
            weatherTasks.Add(task);
        }

        // keep going until valid response
        while (weatherTasks.Count > 0)
        {
            try
            {
                var completedTask = await Task.WhenAny(weatherTasks);
                weatherTasks.Remove(completedTask);

                var result = completedTask.Result;
                if (result is not null)
                {
                    Log.Information($"Found good result from {result.Station}");
                    fastestResult = result;

                    cts.Cancel();
                    canceledTasksCount = weatherTasks.Count;

                    break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(RunAsync)} Error. {ex.Message}");
                throw;
            }
        }
        
        // create message
        var message = fastestResult is not null
            ? $"Fastest station: {fastestResult.Station}. Temperature: {fastestResult?.Hourly?.Temperature2m?.FirstOrDefault()} at {fastestResult?.Hourly?.Time?.FirstOrDefault()}. " +
              $"{canceledTasksCount} tasks were canceled."
            : "No valid weather data was retrieved.";

        return message;
    }
}
