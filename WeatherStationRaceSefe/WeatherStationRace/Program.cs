using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;
using Serilog.Events;
using Shared.Core;
using System.Net.Http.Headers;
using WeatherStationRace;
using WeatherStationRace.Client;
using WeatherStationRace.Client.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var services = new ServiceCollection();

    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    // configure bindings
    services.Configure<MeteoWeatherConfiguration>(configuration.GetSection(nameof(MeteoWeatherConfiguration)));

    // register services
    services.AddTransient<IMeteoWeatherForecastClient, MeteoWeatherForecastClient>();

    services.AddTransient<IMeteoWeatherService, MeteoWeatherService>();

    services.AddSingleton<IWorker, Worker>();

    // register http client
    // meteo weather forecast
    services.AddHttpClient<IMeteoWeatherForecastClient, MeteoWeatherForecastClient>((sp, client) =>
    {
        var configuration = sp.GetRequiredService<IOptions<MeteoWeatherConfiguration>>().Value;
        client.Timeout = TimeSpan.FromMinutes(2);
        client.BaseAddress = new Uri(configuration.BaseUri!);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddResilienceHandler("MeteoClientPolicy", builder =>
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMinutes(2),
            BackoffType = DelayBackoffType.Constant
        });

        builder.AddTimeout(TimeSpan.FromMinutes(2));
    });

    var cts = new CancellationTokenSource();

    // Run services from provider
    var serviceProvider = services.BuildServiceProvider();

    var worker = serviceProvider.GetRequiredService<IWorker>();

    var response = await worker.RunAsync(cts);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}