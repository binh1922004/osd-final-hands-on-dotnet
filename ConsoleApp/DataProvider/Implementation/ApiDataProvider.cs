using System.Text.Json;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using ConsoleApp.Utilities;
using Serilog;

namespace ConsoleApp.DataProvider.Implementation;

public class ApiDataProvider(
    IHttpClientFactory httpClientFactory ) : ICollectionDataProvider
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string apiUrl,CancellationToken cancellationToken) where T : class, new()
    {
        try
        {
            Log.Information("Start reading data from {Provider} source: {Source}", Constant.ApiProviderName, apiUrl);

            var response = await _httpClient.GetAsync(apiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("{Provider} request failed with status code: {StatusCode}, source: {Source}", Constant.ApiProviderName, response.StatusCode, apiUrl);
                throw new HttpRequestException($"API request failed with status code: {response.StatusCode}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Information("Successfully received response from {Provider} source: {Source}", Constant.ApiProviderName, apiUrl);

            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var jsonArray = jsonDocument.RootElement;
            if (jsonArray.ValueKind != JsonValueKind.Array)
            {
                Log.Warning("{Provider} response does not contain an array: {Source}", Constant.ApiProviderName, apiUrl);
                throw new JsonException("API response does not contain an array");
            }
            
            var validItems = new List<T>();
            var skippedCount = 0;
            
            for (var i = 0; i < jsonArray.GetArrayLength(); i++)
            {
                try
                {
                    var element = jsonArray[i];
                    var item = JsonSerializer.Deserialize<T>(element.GetRawText());
                    if (item != null)
                    {
                        validItems.Add(item);
                    }
                }
                catch (JsonException ex)
                {
                    var error = new ErrorRow
                    {
                        Type = DataProviderType.Api,
                        RowNumber = i + 1,
                        ErrorMessage = ex.Message
                    };
                    Log.Error(error.ToString());
                    skippedCount++;
                }
            }

            Log.Information("Successfully loaded {ValidCount} records from {Provider} source, skipped {SkippedCount} invalid records: {Source}", 
                validItems.Count, Constant.ApiProviderName, skippedCount, apiUrl);
            return validItems;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize {Provider} response from source: {Source}", Constant.ApiProviderName, apiUrl);
            throw;
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "HTTP request failed for {Provider} source: {Source}", Constant.ApiProviderName, apiUrl);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Log.Error(ex, "{Provider} request timeout for source: {Source}", Constant.ApiProviderName, apiUrl);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while reading data from {Provider} source: {Source}", Constant.ApiProviderName, apiUrl);
            throw;
        }
    }
}