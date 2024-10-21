using System.Text.Json;
using System.Text;
using Serilog;
using Shared.AotJsonSerializer;
using System.Net;

namespace WeatherStationRace.Client;

public interface IMeteoWeatherForecastClient
{
    public ValueTask<T?> InvokeAsync<T>(
        HttpMethod httpMethod, 
        string apiAction, 
        CancellationToken ct, 
        Dictionary<string, string>? queryStringData = null);
}

public sealed class MeteoWeatherForecastClient : IMeteoWeatherForecastClient
{
    private readonly HttpClient _httpClient;

    public MeteoWeatherForecastClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<T?> InvokeAsync<T>(
        HttpMethod httpMethod, 
        string apiAction, 
        CancellationToken ct, 
        Dictionary<string, string>? queryStringData = null)
    {
        try
        {
            var requestUrl = BuildRequestUrl(apiAction, queryStringData);

            ct.ThrowIfCancellationRequested();

            HttpResponseMessage response = await _httpClient!.SendAsync(new(httpMethod, requestUrl), ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // token stuff
            }

            if (response.IsSuccessStatusCode)
            {
                string jsonStringFromResponse = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(jsonStringFromResponse))
                {
                    var context = JsonSerializerContextRegistry.GetContext(typeof(T));

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        TypeInfoResolver = context
                    };

                    return JsonSerializer.Deserialize<T>(jsonStringFromResponse, options);
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            Log.Warning($"{nameof(InvokeAsync)} canceled. {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(InvokeAsync)} Error. {ex.Message}");
            throw;
        }

        return default;
    }

    private static string BuildRequestUrl(
        string apiAction, 
        Dictionary<string, string>? queryStringData)
    {
        StringBuilder endpointBuilder = new();
        endpointBuilder.Append(apiAction);

        string requestUrl = endpointBuilder.ToString();

        if (queryStringData?.Count > 0)
        {
            var queryString = string.Join("&", queryStringData.Select(kv => $"{kv.Key}={kv.Value}"));
            requestUrl += $"?{queryString}";
        }

        return requestUrl;
    }
}
