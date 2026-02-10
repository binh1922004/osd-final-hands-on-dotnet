using System.Text.Json;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.DataProvider.Implementation;

public class ApiDataProvider(
    ILogger<ApiDataProvider> logger,
    IConfiguration configuration,
    HttpClient httpClient) : IDataProvider
{
    

    public async Task<T> GetDataAsync<T>(string source)
    {
        try
        {
            logger.LogInformation("Start fetching product data from API: {ApiUrl}", source);

            var response = await httpClient.GetAsync(source);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("API request failed with status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"API request failed with status code: {response.StatusCode}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Successfully received response from API: {ApiUrl}", source);

            var jsonData = JsonSerializer.Deserialize<T>(jsonContent);
            
            logger.LogInformation("Successfully parse json from API");
            return jsonData;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize API response from: {ApiUrl}", source);
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed for API: {ApiUrl}", source);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "API request timeout for: {ApiUrl}", source);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching product data from API: {ApiUrl}", source);
            throw;
        }
    }
}